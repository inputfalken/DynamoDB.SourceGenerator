using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.DynamoDBDocument;

public class DynamoDbMarshaller
{
    private const string DeserializeName = "Unmarshall";
    private const string InterfaceName = "IDynamoDBMarshaller";
    private const string KeysName = "Keys";
    private const string PartitionKeyName = "PartitionKey";
    private const string RangeKeyName = "RangeKey";
    private const string ValueTrackerName = "AttributeExpressionValueTracker";
    private const string NameTrackerName = "AttributeNameExpressionTracker";
    private const string SerializeName = "Marshall";
    private readonly IEqualityComparer<ITypeSymbol> _comparer;
    private readonly Func<ITypeSymbol, string> _deserializationMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _fullTypeNameFactory;
    private readonly Func<ITypeSymbol, string> _serializationMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _keysMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _attributeNameAssignmentNameFactory;
    private readonly Func<ITypeSymbol, string> _attributeValueAssignmentNameFactory;
    private readonly INamedTypeSymbol _entityTypeSymbol;
    private readonly INamedTypeSymbol _argumentTypeSymbol;
    private readonly string _publicAccessPropertyName;

    public DynamoDbMarshaller(in DynamoDBMarshallerArguments arguments, IEqualityComparer<ISymbol> comparer)
    {
        _entityTypeSymbol = (INamedTypeSymbol)arguments.EntityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        _argumentTypeSymbol = (INamedTypeSymbol)arguments.ArgumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        _publicAccessPropertyName = arguments.PropertyName;
        _serializationMethodNameFactory = TypeExtensions.TypeSymbolStringCache("Ser", comparer, true);
        _keysMethodNameFactory = TypeExtensions.TypeSymbolStringCache("Keys", comparer, false);
        _deserializationMethodNameFactory = TypeExtensions.TypeSymbolStringCache("Des", comparer, true);
        _attributeNameAssignmentNameFactory = TypeExtensions.TypeSymbolStringCache("Name", comparer, false);
        _attributeValueAssignmentNameFactory = TypeExtensions.TypeSymbolStringCache("Value", comparer, false);
        _fullTypeNameFactory = TypeExtensions.NameCache(SymbolDisplayFormat.FullyQualifiedFormat, comparer);
        _comparer = comparer;
    }
    private Assignment AttributeValueAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        if (typeSymbol.GetKnownType() is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

        var assignment = knownType switch
        {
            BaseType baseType => baseType.Type switch
            {
                BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"S = {accessPattern}"),
                BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"BOOL = {accessPattern}"),
                BaseType.SupportedType.Int16
                    or BaseType.SupportedType.Int32
                    or BaseType.SupportedType.Int64
                    or BaseType.SupportedType.UInt16
                    or BaseType.SupportedType.UInt32
                    or BaseType.SupportedType.UInt64
                    or BaseType.SupportedType.Double
                    or BaseType.SupportedType.Decimal
                    or BaseType.SupportedType.Single
                    or BaseType.SupportedType.SByte
                    or BaseType.SupportedType.Byte
                    => typeSymbol.ToInlineAssignment($"N = {accessPattern}.ToString()"),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"S = {accessPattern}.ToString()"),
                BaseType.SupportedType.DateOnly or BaseType.SupportedType.DateTimeOffset or BaseType.SupportedType.DateTime => typeSymbol.ToInlineAssignment($@"S = {accessPattern}.ToString(""O"")"),
                BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"N = ((int){accessPattern}).ToString()"),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => AttributeValueAssignment(singleGeneric.T, $"{accessPattern}.Value"),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.Array
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => BuildList(singleGeneric.T, in accessPattern),
                SingleGeneric.SupportedType.Set => BuildSet(singleGeneric.T, accessPattern),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            KeyValueGeneric keyValueGeneric => StringKeyedValuedGeneric(in keyValueGeneric, in accessPattern),
            _ => null
        };

        return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

        Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
            typeSymbol.ToExternalDependencyAssignment($"M = {_serializationMethodNameFactory(typeSymbol)}({accessPattern})");
    }

    private Assignment BuildList(in ITypeSymbol elementType, in string accessPattern)
    {
        var innerAssignment = AttributeValueAssignment(elementType, "x");
        var select = $"Select(x => {innerAssignment.ToAttributeValue()}))";
        var outerAssignment = elementType.NotNullLambdaExpression() is { } whereBody
            ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
            : $"L = new List<AttributeValue>({accessPattern}.{select}";

        return new Assignment(in outerAssignment, in elementType, innerAssignment.HasExternalDependency);
    }

    private Assignment BuildPocoList(in SingleGeneric singleGeneric, in string? operation, in string accessPattern, in string defaultCause)
    {
        var innerAssignment = DataMemberAssignment(singleGeneric.T, "y");
        var outerAssignment = $"{accessPattern} switch {{ {{ L: {{ }} x }} => x.Select(y => {innerAssignment.Value}){operation}, {defaultCause} }}";

        return new Assignment(in outerAssignment, singleGeneric.T, innerAssignment.HasExternalDependency);
    }

    private static Assignment? BuildPocoSet(in ITypeSymbol elementType, in string accessPattern, in string defaultCase)
    {
        if (elementType.IsNumeric())
            return elementType.ToInlineAssignment($"{accessPattern} switch {{ {{ NS: {{ }} x }} =>  new HashSet<{elementType.Name}>(x.Select(y => {elementType.Name}.Parse(y))), {defaultCase} }}");

        if (elementType.SpecialType is SpecialType.System_String)
            return elementType.ToInlineAssignment($"{accessPattern} switch {{ {{ SS: {{ }} x }} =>  new HashSet<string>(x), {defaultCase} }}");

        return null;
    }
    private static Assignment? BuildSet(in ITypeSymbol elementType, in string accessPattern)
    {
        var newAccessPattern = elementType.NotNullLambdaExpression() is { } expression
            ? $"{accessPattern}.Where({expression})"
            : accessPattern;

        if (elementType.SpecialType is SpecialType.System_String)
            return elementType.ToInlineAssignment($"SS = new List<string>({newAccessPattern})");

        return elementType.IsNumeric() is false
            ? null
            : elementType.ToInlineAssignment($"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))");
    }
    private string CreateAttributePocoFactory()
    {
        var enumerable = Conversion.ConversionMethods(
                _entityTypeSymbol,
                StaticPocoFactory,
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Select(static x => x.Code);

        return string.Join(Constants.NewLine, enumerable);
    }

    private string CreateAttributeValueFactory(ITypeSymbol typeSymbol, KeyStrategy keyStrategy, ISet<ITypeSymbol> typeSymbols)
    {
        var enumerable = Conversion.ConversionMethods(
                typeSymbol,
                x => StaticAttributeValueDictionaryFactory(x, keyStrategy),
                typeSymbols
            )
            .Select(static x => x.Code);

        return string.Join(Constants.NewLine, enumerable);
    }


    public string CreateDynamoDbDocumentProperty(Accessibility accessibility)
    {

        var referenceTrackers = Conversion.ConversionMethods(
                _entityTypeSymbol,
                ExpressionAttributeName,
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Concat(Conversion.ConversionMethods(
                    _argumentTypeSymbol,
                    ExpressionAttributeValue,
                    new HashSet<ITypeSymbol>(_comparer)
                )
            )
            .Select(static x => x.Code);

        var className = $"{_publicAccessPropertyName}Implementation";

        var set = new HashSet<ITypeSymbol>(_comparer);
        var marshalMethods = CreateAttributeValueFactory(_entityTypeSymbol, KeyStrategy.Include, set);
        var argumentMarshallMethods = CreateAttributeValueFactory(_argumentTypeSymbol, KeyStrategy.Include, set);
        var keysMethod = StaticAttributeValueDictionaryKeys(_entityTypeSymbol, "partition", "range");

        var unMarshalMethods = CreateAttributePocoFactory();

        var rootTypeName = _fullTypeNameFactory(_entityTypeSymbol);
        var valueTrackerTypeName = _attributeValueAssignmentNameFactory(_argumentTypeSymbol);
        var nameTrackerTypeName = _attributeNameAssignmentNameFactory(_entityTypeSymbol);
        var implementInterface =
            $@"public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {SerializeName}({rootTypeName} entity) => {_serializationMethodNameFactory(_entityTypeSymbol)}(entity);
            public {rootTypeName} {DeserializeName}({nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> entity) => {_deserializationMethodNameFactory(_entityTypeSymbol)}(entity);
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {KeysName}(object partitionKey, object rangeKey) => {_keysMethodNameFactory(_entityTypeSymbol)}((partitionKey, true), (rangeKey, true));
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {RangeKeyName}(object rangeKey) => {_keysMethodNameFactory(_entityTypeSymbol)}(null, (rangeKey, true));
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {PartitionKeyName}(object partitionKey) => {_keysMethodNameFactory(_entityTypeSymbol)}((partitionKey, true), null);
            public {className}.{valueTrackerTypeName} {ValueTrackerName}()
            {{
                var number = 0;
                Func<string> valueIdProvider = () => $"":p{{++number}}"";
                return new {className}.{valueTrackerTypeName}(valueIdProvider);
            }}
            public {className}.{nameTrackerTypeName} {NameTrackerName}()
            {{
                return new {className}.{nameTrackerTypeName}(null);
            }}
{marshalMethods}
{argumentMarshallMethods}
{keysMethod}
{unMarshalMethods}";

        var sourceGeneratedCode = string.Join(Constants.NewLine, referenceTrackers.Prepend(implementInterface));

        var @interface =
            $"{InterfaceName}<{rootTypeName}, {_fullTypeNameFactory(_argumentTypeSymbol)}, {className}.{nameTrackerTypeName}, {className}.{valueTrackerTypeName}>";
        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className}: {@interface}",
            in sourceGeneratedCode,
            2
        );

        return
            $@"{accessibility.ToCode()} {@interface} {_publicAccessPropertyName} {{ get; }} = new {className}();
        {@class}";
    }

    private Assignment DataMemberAssignment(in ITypeSymbol type, in string pattern)
    {
        var defaultCase = type.IsNullable() ? "_ => null" : @$"_ => throw new ArgumentNullException(""{Constants.NotNullErrorMessage}"")";
        return Execution(in type, in pattern, defaultCase);

        Assignment Execution(in ITypeSymbol typeSymbol, in string accessPattern, string @default)
        {
            if (typeSymbol.GetKnownType() is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

            var assignment = knownType switch
            {
                BaseType baseType => baseType.Type switch
                {
                    BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x, {@default} }}"),
                    BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ BOOL: var x }} => x, {@default} }}"),
                    BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x[0], {@default} }}"),
                    BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} when Int32.Parse(x) is var y =>({_fullTypeNameFactory(typeSymbol)})y, {@default} }}"),
                    BaseType.SupportedType.Int16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int16.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Byte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Byte.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Int32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int32.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Int64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int64.Parse(x), {@default} }}"),
                    BaseType.SupportedType.SByte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => SByte.Parse(x), {@default} }}"),
                    BaseType.SupportedType.UInt16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt16.Parse(x), {@default} }}"),
                    BaseType.SupportedType.UInt32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt32.Parse(x), {@default} }}"),
                    BaseType.SupportedType.UInt64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{  {{ N: {{ }} x }} => UInt64.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Decimal => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Decimal.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Double => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Double.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Single => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Single.Parse(x), {@default} }}"),
                    BaseType.SupportedType.DateTime => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTime.Parse(x), {@default} }}"),
                    BaseType.SupportedType.DateTimeOffset => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTimeOffset.Parse(x), {@default} }}"),
                    BaseType.SupportedType.DateOnly => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateOnly.Parse(x), {@default} }}"),
                    _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
                },
                SingleGeneric singleGeneric => singleGeneric.Type switch
                {
                    SingleGeneric.SupportedType.Nullable => Execution(singleGeneric.T, in accessPattern, @default),
                    SingleGeneric.SupportedType.Set => BuildPocoSet(singleGeneric.T, in accessPattern, in @default),
                    SingleGeneric.SupportedType.Array => BuildPocoList(in singleGeneric, ".ToArray()", in accessPattern, in @default),
                    SingleGeneric.SupportedType.ICollection => BuildPocoList(in singleGeneric, ".ToList()", in accessPattern, in @default),
                    SingleGeneric.SupportedType.IReadOnlyCollection => BuildPocoList(in singleGeneric, ".ToArray()", in accessPattern, in @default),
                    SingleGeneric.SupportedType.IEnumerable => BuildPocoList(in singleGeneric, null, in accessPattern, in @default),
                    _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
                },
                KeyValueGeneric keyValueGeneric => StringKeyedPocoGeneric(in keyValueGeneric, in accessPattern, @default),
                _ => null
            };

            return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

            Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
                typeSymbol.ToExternalDependencyAssignment($"{accessPattern} switch {{ {{ M: {{ }} x }} => {_deserializationMethodNameFactory(typeSymbol)}(x), {@default} }}");
        }

    }

    private Conversion ExpressionAttributeValue(ITypeSymbol typeSymbol)
    {
        var dataMembers = typeSymbol
            .GetDynamoDbProperties()
            .Where(static x => x.IsIgnored is false)
            .Select(x => (
                IsKnown: x.DataMember.Type.GetKnownType() is not null,
                DDB: x,
                ValueRef: $"_{x.DataMember.Name}ValueRef",
                AttributeReference: _attributeValueAssignmentNameFactory(x.DataMember.Type),
                AttributeInterfaceName: AttributeInterfaceName(x.DataMember.Type)))
            .ToArray();

        var fieldDeclarations = dataMembers
            .Select(static x => x.IsKnown
                ? $@"    private readonly Lazy<string> {x.ValueRef};
    public string {x.DDB.DataMember.Name} => {x.ValueRef}.Value;"
                : $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};
    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;"
            );

        const string valueProvider = "valueIdProvider";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                var assignment = x.IsKnown
                    ? $@"        {x.ValueRef} = new ({valueProvider});"
                    : $@"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({valueProvider}));";

                return new Assignment(assignment, x.DDB.DataMember.Type, x.IsKnown is false);
            })
            .ToArray();

        var className = _attributeValueAssignmentNameFactory(typeSymbol);
        var constructor = $@"public {className}(Func<string> {valueProvider})
    {{
{string.Join(Constants.NewLine, fieldAssignments.Select(x => x.Value))}
    }}";

        var expressionAttributeValueYields = dataMembers
            .Select(x =>
            {
                var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                return x.IsKnown
                    ? $@"       if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {AttributeValueAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}"
                    : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{nameof(IExpressionAttributeValueTracker<object>.AccessedValues)}({accessPattern})) {{ yield return x; }}")}";
            });

        var interfaceName = AttributeInterfaceName(typeSymbol);
        var @class = CodeGenerationExtensions.CreateStruct(
            Accessibility.Public,
            $"{className} : {interfaceName}",
            $@"{constructor}
{string.Join(Constants.NewLine, fieldDeclarations)}
    IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{nameof(IExpressionAttributeValueTracker<int>.AccessedValues)}({_fullTypeNameFactory(typeSymbol)} entity)
    {{
{(string.Join(Constants.NewLine, expressionAttributeValueYields) is var joinedValues && joinedValues != string.Empty ? joinedValues : "return Enumerable.Empty<KeyValuePair<string, AttributeValue>>();")}
    }}",
            0,
            isReadonly: true,
            isRecord: true
        );
        return new Conversion(@class, fieldAssignments.Where(x => x.HasExternalDependency));

        string AttributeInterfaceName(ITypeSymbol symbol) => $"{nameof(IExpressionAttributeValueTracker<object>)}<{_fullTypeNameFactory(symbol)}>";
    }
    private Conversion ExpressionAttributeName(ITypeSymbol typeSymbol)
    {

        var dataMembers = typeSymbol
            .GetDynamoDbProperties()
            .Where(static x => x.IsIgnored is false)
            .Select(x => (
                IsKnown: x.DataMember.Type.GetKnownType() is not null,
                DDB: x,
                NameRef: $"_{x.DataMember.Name}NameRef",
                AttributeReference: _attributeNameAssignmentNameFactory(x.DataMember.Type),
                AttributeInterfaceName: AttributeInterfaceName()))
            .ToArray();

        var fieldDeclarations = dataMembers
            .Select(static x => x.IsKnown
                ? $@"    private readonly Lazy<string> {x.NameRef};
    public string {x.DDB.DataMember.Name} => {x.NameRef}.Value;"
                : $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};
    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;"
            );

        const string constructorAttributeName = "nameRef";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                var ternaryExpressionName =
                    $"{constructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                var assignment = x.IsKnown
                    ? $@"        {x.NameRef} = new (() => {ternaryExpressionName});"
                    : $@"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ternaryExpressionName}));";

                return new Assignment(assignment, x.DDB.DataMember.Type, x.IsKnown is false);
            })
            .ToArray();

        var className = _attributeNameAssignmentNameFactory(typeSymbol);
        var constructor = $@"public {className}(string? {constructorAttributeName})
    {{
{string.Join(Constants.NewLine, fieldAssignments.Select(x => x.Value))}
    }}";

        var expressionAttributeNameYields = dataMembers.Select(static x => x.IsKnown
            ? $@"       if ({x.NameRef}.IsValueCreated) yield return new ({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
            : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{nameof(IExpressionAttributeNameTracker.AccessedNames)}()) {{ yield return x; }}");

        var interfaceName = AttributeInterfaceName();
        var @class = CodeGenerationExtensions.CreateStruct(
            Accessibility.Public,
            $"{className} : {interfaceName}",
            $@"{constructor}
{string.Join(Constants.NewLine, fieldDeclarations)}
    IEnumerable<KeyValuePair<string, string>> {interfaceName}.{nameof(IExpressionAttributeNameTracker.AccessedNames)}()
    {{
{(string.Join(Constants.NewLine, expressionAttributeNameYields) is var joinedNames && joinedNames != string.Empty ? joinedNames : "return Enumerable.Empty<KeyValuePair<string, string>>();")}
    }}",
            0,
            isReadonly: true,
            isRecord: true
        );
        return new Conversion(@class, fieldAssignments.Where(x => x.HasExternalDependency));

        string AttributeInterfaceName() => nameof(IExpressionAttributeNameTracker);
    }
    private string StaticAttributeValueDictionaryKeys(
        ITypeSymbol type,
        string partitionKeyReference,
        string rangeKeyReference)
    {
        const string dictionaryName = "attributeValues";

        var properties = GetAssignments(partitionKeyReference, rangeKeyReference, type).ToArray();

        const string indent = "                ";
        var body = properties.Length is 0
            ? @$"throw new InvalidOperationException(""Could not create keys for type '{type.Name}', include DynamoDBKeyAttribute on the correct properties."");"
            : @$"{InitializeDictionary(dictionaryName, properties.Select(static x => x.capacityTernary))}
                {string.Join(Constants.NewLine + indent, properties.Select(static x => x.dictionaryPopulation))}
                if ({dictionaryName}.Count != (({partitionKeyReference} is null ? 0 : 1) + ({rangeKeyReference} is null ? 0 : 1)))
                    throw new InvalidOperationException(""The amount of keys does not match the amount provided."");
                return {dictionaryName};";
        var method =
            @$"            public static Dictionary<string, AttributeValue> {_keysMethodNameFactory(type)}((object Value, bool IsStrict)? {partitionKeyReference}, (object Value, bool IsStrict)? {rangeKeyReference})
            {{
                {body}
            }}";

        return method;

        IEnumerable<(string dictionaryPopulation, string capacityTernary, Assignment assignment)> GetAssignments(
            string partition,
            string range,
            INamespaceOrTypeSymbol typeSymbol
        )
        {
            foreach (var x in typeSymbol.GetDynamoDbProperties())
            {
                if (x.IsIgnored)
                    continue;

                string accessPattern;
                if (x.IsHashKey)
                    accessPattern = partition;
                else if (x.IsRangeKey)
                    accessPattern = range;
                else
                    continue;

                var reference = $"converted{x.AttributeName}";
                var declaration = $"var {reference} = {accessPattern}?.Value as {(x.DataMember.Type.IsValueType ? $"{_fullTypeNameFactory(x.DataMember.Type)}?" : _fullTypeNameFactory(x.DataMember.Type))};";
                var attributeConversion = AttributeValueAssignment(x.DataMember.Type, reference);

                var assignment = x.DataMember.Type.NotNullIfStatement(
                    in reference,
                    @$"{dictionaryName}.Add(""{x.AttributeName}"", {attributeConversion.ToAttributeValue()});"
                );

                var capacityTernaries = x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0");

                yield return ($@"{declaration}
                if({accessPattern}?.IsStrict == true)
                    {assignment}", capacityTernaries, attributeConversion);
            }
        }

        static string InitializeDictionary(string dictionaryName, IEnumerable<string> capacityCalculations)
        {
            var capacityCalculation = string.Join(" + ", capacityCalculations);

            return string.Join(" + ", capacityCalculation) switch
            {
                "" => $"var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: 0);",
                var capacities => $@"var capacity = {capacities};
                var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: capacity);"
            };
        }
    }
    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type, KeyStrategy keyStrategy)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = GetAssignments(type, keyStrategy).ToArray();

        const string indent = "                ";
        var method =
            @$"            public static Dictionary<string, AttributeValue> {_serializationMethodNameFactory(type)}({_fullTypeNameFactory(type)} {paramReference})
            {{
                {InitializeDictionary(dictionaryName, properties.Select(static x => x.capacityTernary))}
                {string.Join(Constants.NewLine + indent, properties.Select(static x => x.dictionaryPopulation))}
                return {dictionaryName};
            }}";

        return new Conversion(
            in method,
            properties
                .Select(static x => x.assignment)
                .Where(static x => x.HasExternalDependency)
        );

        IEnumerable<(string dictionaryPopulation, string capacityTernary, Assignment assignment)> GetAssignments(
            INamespaceOrTypeSymbol typeSymbol,
            KeyStrategy strategy
        )
        {
            var dynamoDbProperties = strategy switch
            {
                KeyStrategy.Ignore => typeSymbol.GetDynamoDbProperties()
                    .Where(static x => x is {IsRangeKey: false, IsHashKey: false}),
                KeyStrategy.Only => typeSymbol.GetDynamoDbProperties()
                    .Where(static x => x.IsHashKey || x.IsRangeKey),
                KeyStrategy.Include => typeSymbol.GetDynamoDbProperties(),
                _ => throw new ArgumentOutOfRangeException()
            };

            foreach (var x in dynamoDbProperties)
            {
                if (x.IsIgnored)
                    continue;

                var accessPattern = $"{paramReference}.{x.DataMember.Name}";
                var attributeValue = AttributeValueAssignment(x.DataMember.Type, accessPattern);
                if (strategy == KeyStrategy.Only && attributeValue.HasExternalDependency)
                    continue;

                var dictionaryAssignment = x.DataMember.Type.NotNullIfStatement(
                    in accessPattern,
                    @$"{dictionaryName}.Add(""{x.AttributeName}"", {attributeValue.ToAttributeValue()});"
                );

                var capacityTernaries = x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0");

                yield return (dictionaryAssignment, capacityTernaries, attributeValue);
            }
        }

        static string InitializeDictionary(string dictionaryName, IEnumerable<string> capacityCalculations)
        {
            var capacityCalculation = string.Join(" + ", capacityCalculations);

            return string.Join(" + ", capacityCalculation)switch
            {
                "" => $"var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: 0);",
                var capacities => $@"var capacity = {capacities};
                var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: capacity);
                if (capacity is 0) {{ return {dictionaryName}; }} "
            };
        }
    }

    private Conversion StaticPocoFactory(ITypeSymbol type)
    {

        var values = GetAssignments(type);
        const string paramReference = "entity";
        var method =
            @$"            public static {_fullTypeNameFactory(type)} {_deserializationMethodNameFactory(type)}(Dictionary<string, AttributeValue> {paramReference})
            {{ 
                return {values.objectInitialization};
            }}";

        return new Conversion(method, values.Item1.Where(x => x.HasExternalDependency));

        (IEnumerable<Assignment>, string objectInitialization) GetAssignments(ITypeSymbol typeSymbol)
        {
            var assignments = typeSymbol
                .GetDynamoDbProperties()
                .Select(x => (DDB: x, Assignment: DataMemberAssignment(x.DataMember.Type, @$"{paramReference}.GetValueOrDefault(""{x.AttributeName}"")")))
                .ToArray();

            if (typeSymbol.IsTupleType)
                return (assignments.Select(x => x.Assignment), $"({string.Join(", ", assignments.Select(x => $"{x.DDB.DataMember.Name}: {x.Assignment.Value}"))})");

            var objectArguments = assignments
                .GroupJoin(
                    TryGetMatchedConstructorArguments(typeSymbol),
                    x => x.DDB.DataMember.Name,
                    x => x.DataMember,
                    (x, y) => (x.Assignment, x.DDB, Constructor: y.OfType<(string DataMember, string ParameterName)?>().FirstOrDefault()),
                    StringComparer.OrdinalIgnoreCase // Is Required for KeyValuePair to work.
                )
                .GroupBy(x => x.Constructor.HasValue, (x, y) =>
                {
                    return x
                        ? @$"
                (
{string.Join($",{Constants.NewLine}", y.Select(z => $"                    {z.Constructor!.Value.ParameterName} : {z.Assignment.Value}"))}
                )"
                        : $@"
                {{
{string.Join($",{Constants.NewLine}", y.Where(x => x.DDB.DataMember.IsAssignable).Select(z => $"                    {z.DDB.DataMember.Name} = {z.Assignment.Value}"))}
                }}";
                });

            return (
                assignments.Select(x => x.Assignment),
                string.Join("", objectArguments) switch
                {
                    "" => $"new {_fullTypeNameFactory(typeSymbol)}()",
                    var args => $"new {_fullTypeNameFactory(typeSymbol)}{args}"
                }
            );
        }

        static IEnumerable<(string DataMember, string ParameterName)> TryGetMatchedConstructorArguments(ITypeSymbol typeSymbol)
        {

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                return Enumerable.Empty<(string, string )>();

            if (namedTypeSymbol.InstanceConstructors.Length is 0)
                return Enumerable.Empty<(string, string )>();

            if (namedTypeSymbol is {Name: "KeyValuePair", ContainingNamespace.Name: "Generic"})
                return namedTypeSymbol.InstanceConstructors
                    .First(x => x.Parameters.Length is 2)
                    .Parameters
                    .Select(x => (MemberName: x.Name, ParameterName: x.Name));

            // Should not need to be looked at when it's a RecordDeclarationSyntax
            return namedTypeSymbol switch
            {
                _ when namedTypeSymbol.InstanceConstructors
                    .SelectMany(
                        x => x.GetAttributes().Where(y => y.AttributeClass is
                        {
                            ContainingNamespace.Name: Constants.AttributeNameSpace,
                            Name: Constants.MarshallerConstructorAttributeName,
                            ContainingAssembly.Name: Constants.AssemblyName
                        }),
                        (x, _) => x
                    )
                    .FirstOrDefault() is { } ctor => ctor.DeclaringSyntaxReferences
                    .Select(x => x.GetSyntax())
                    .OfType<ConstructorDeclarationSyntax>()
                    .Select(
                        x => x.DescendantNodes()
                            .OfType<AssignmentExpressionSyntax>()
                            .Select(y => (MemberName: y.Left.ToString(), ParameterName: y.Right.ToString()))
                    )
                    .FirstOrDefault() ?? Enumerable.Empty<(string, string)>(),
                {IsRecord: true} when namedTypeSymbol.InstanceConstructors[0]
                    .DeclaringSyntaxReferences
                    .Select(x => x.GetSyntax())
                    .OfType<RecordDeclarationSyntax>()
                    .FirstOrDefault() is { } ctor => ctor
                    .DescendantNodes()
                    .OfType<ParameterSyntax>()
                    .Select(x => (MemberName: x.Identifier.Text, ParameterName: x.Identifier.Text)),
                _ => Array.Empty<(string, string)>()
            };

        }

    }

    private Assignment? StringKeyedPocoGeneric(in KeyValueGeneric keyValueGeneric, in string accessPattern, string defaultCase)
    {
        switch (keyValueGeneric)
        {
            case {TKey: not {SpecialType: SpecialType.System_String}}:
                return null;
            case {Type: KeyValueGeneric.SupportedType.LookUp}:
                var lookupValueAssignment = DataMemberAssignment(keyValueGeneric.TValue, "y.z");
                return new Assignment(
                    $"{accessPattern} switch {{ {{ M: {{ }} x }} => x.SelectMany(y => y.Value.L, (y, z) => (y.Key, z)).ToLookup(y => y.Key, y => {lookupValueAssignment.Value}), {defaultCase} }}",
                    keyValueGeneric.TValue,
                    lookupValueAssignment.HasExternalDependency
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = DataMemberAssignment(keyValueGeneric.TValue, "y.Value");
                return new Assignment(
                    $@"{accessPattern} switch {{ {{ M: {{ }} x }} => x.ToDictionary(y => y.Key, y => {dictionaryValueAssignment.Value}), {defaultCase} }}",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.HasExternalDependency
                );
            default:
                return null;
        }
    }
    private Assignment? StringKeyedValuedGeneric(in KeyValueGeneric keyValueGeneric, in string accessPattern)
    {
        switch (keyValueGeneric)
        {
            case {TKey: not {SpecialType: SpecialType.System_String}}:
                return null;
            case {Type: KeyValueGeneric.SupportedType.LookUp}:
                var lookupValueAssignment = BuildList(keyValueGeneric.TValue, "x");
                return new Assignment(
                    $"M = {accessPattern}.ToDictionary(x => x.Key, x => {lookupValueAssignment.ToAttributeValue()})",
                    keyValueGeneric.TValue,
                    lookupValueAssignment.HasExternalDependency
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = AttributeValueAssignment(keyValueGeneric.TValue, "x.Value");
                return new Assignment(
                    $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {dictionaryValueAssignment.ToAttributeValue()})",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.HasExternalDependency
                );
            default:
                return null;
        }
    }
}