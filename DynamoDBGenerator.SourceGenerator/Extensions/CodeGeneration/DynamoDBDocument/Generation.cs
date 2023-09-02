using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.DynamoDBDocument;

public class DynamoDbDocumentGenerator
{
    private const string DeserializeName = "Deserialize";
    private const string DynamoDbDocumentName = "IDynamoDBDocument";
    private const string KeysName = "Keys";
    private const string ValueTrackerName = "AttributeExpressionValueTracker";
    private const string NameTrackerName = "AttributeNameExpressionTracker";
    private const string SerializeName = "Serialize";
    private readonly IEqualityComparer<ITypeSymbol> _comparer;
    private readonly Func<ITypeSymbol, string> _deserializationMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _fullTypeNameFactory;
    private readonly Func<ITypeSymbol, string> _serializationMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _attributeNameAssignmentNameFactory;
    private readonly Func<ITypeSymbol, string> _attributeValueAssignmentNameFactory;
    private readonly INamedTypeSymbol _entityTypeSymbol;
    private readonly INamedTypeSymbol _argumentTypeSymbol;
    private readonly string _publicAccessPropertyName;

    public DynamoDbDocumentGenerator(in DynamoDBDocumentArguments arguments, IEqualityComparer<ISymbol> comparer)
    {
        _entityTypeSymbol = (INamedTypeSymbol)arguments.EntityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        _argumentTypeSymbol = (INamedTypeSymbol)arguments.ArgumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        _publicAccessPropertyName = arguments.PropertyName;
        _serializationMethodNameFactory = TypeExtensions.TypeSymbolStringCache("Ser", comparer);
        _deserializationMethodNameFactory = TypeExtensions.TypeSymbolStringCache("Des", comparer);
        _attributeNameAssignmentNameFactory = TypeExtensions.TypeSymbolStringCache("Name", comparer);
        _attributeValueAssignmentNameFactory = TypeExtensions.TypeSymbolStringCache("Value", comparer);
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

    private (string code, ISet<ITypeSymbol> supportedTypes) CreateAttributeValueFactory(ITypeSymbol typeSymbol, KeyStrategy keyStrategy, ISet<ITypeSymbol>? typeSymbols = null)
    {
        typeSymbols ??= new HashSet<ITypeSymbol>(_comparer);
        var enumerable = Conversion.ConversionMethods(
                typeSymbol,
                x => StaticAttributeValueDictionaryFactory(x, keyStrategy),
                typeSymbols
            )
            .Select(static x => x.Code);

        return (string.Join(Constants.NewLine, enumerable), typeSymbols);

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

        var className = $"{_entityTypeSymbol.Name}_Document";
        //TODO If `_argumentTypeSymbol` would differ from `_entityTypeSymbol` we would need to avoid duplicated methods from being generated.
        var (marshalMethods, _) = CreateAttributeValueFactory(_entityTypeSymbol, KeyStrategy.Include);
        var (keysMethod, _) = CreateAttributeValueFactory(_entityTypeSymbol, KeyStrategy.Only);
        var keysClass = CodeGenerationExtensions.CreateClass(Accessibility.Private, "KeysClass", keysMethod, 2);
        var unMarshalMethods = CreateAttributePocoFactory();

        var rootTypeName = _fullTypeNameFactory(_entityTypeSymbol);
        var valueTrackerTypeName = _attributeValueAssignmentNameFactory(_argumentTypeSymbol);
        var nameTrackerTypeName = _attributeNameAssignmentNameFactory(_entityTypeSymbol);
        var implementInterface =
            $@"public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {SerializeName}({rootTypeName} entity) => {_serializationMethodNameFactory(_entityTypeSymbol)}(entity);
            public {rootTypeName} {DeserializeName}({nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> entity) => {_deserializationMethodNameFactory(_entityTypeSymbol)}(entity);
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {KeysName}({rootTypeName} entity) => KeysClass.{_serializationMethodNameFactory(_entityTypeSymbol)}(entity);
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
{keysClass}
{unMarshalMethods}";

        var sourceGeneratedCode = string.Join(Constants.NewLine, referenceTrackers.Prepend(implementInterface));

        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className}: {DynamoDbDocumentName}<{_entityTypeSymbol.Name}, {_argumentTypeSymbol.Name}, {className}.{nameTrackerTypeName}, {className}.{valueTrackerTypeName}>",
            in sourceGeneratedCode,
            2
        );

        return $@"{accessibility.ToCode()} {DynamoDbDocumentName}<{_entityTypeSymbol.Name}, {_argumentTypeSymbol.Name}, {className}.{nameTrackerTypeName}, {className}.{valueTrackerTypeName}> {_publicAccessPropertyName} {{ get; }} = new {className}();
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
            .Select(static x =>
            {
                if (x.IsKnown)
                {
                    return $@"    private readonly Lazy<string> {x.ValueRef};
    public string {x.DDB.DataMember.Name} => {x.ValueRef}.Value;";
                }
                return $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};
    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;";
            });

        const string valueProvider = "valueIdProvider";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                string assignment;
                if (x.IsKnown)
                {
                    assignment =
                        $@"        {x.ValueRef} = new ({valueProvider});";
                }
                else
                {
                    assignment =
                        $@"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({valueProvider}));";
                }

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
            .Select(static x =>
            {
                if (x.IsKnown)
                {
                    return $@"    private readonly Lazy<string> {x.NameRef};
    public string {x.DDB.DataMember.Name} => {x.NameRef}.Value;";
                }
                return $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};
    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;";
            });

        const string constructorAttributeName = "nameRef";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                string assignment;
                var ternaryExpressionName =
                    $"{constructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                if (x.IsKnown)
                {
                    assignment =
                        $@"        {x.NameRef} = new (() => {ternaryExpressionName});";
                }
                else
                {
                    assignment =
                        $@"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ternaryExpressionName}));";
                }

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

            // Right now we either take a constructor path or object initialization path. But they could co-exist.
            // We do expect the constructor arguments to be 1-1 with case insensitive comparison for data member names.
            var constructorArgs = string.Empty;
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.InstanceConstructors.Any(x => x.Parameters.Length > 0))
            {
                var ctor = namedTypeSymbol.InstanceConstructors
                    .OrderByDescending(x => x.Parameters.Length)
                    .First();

                var ctorInitialization = ctor.Parameters
                    .GroupJoin(
                        assignments,
                        y => y.Name,
                        y => y.DDB.DataMember.Name,
                        (y, z) => (constructurArgument: y, Items: z),
                        StringComparer.OrdinalIgnoreCase
                    )
                    .SelectMany(y => y.Items.Cast<(DynamoDbDataMember DynamoDbDataMember, Assignment assignment)?>().DefaultIfEmpty(), (y, z) => (y.constructurArgument, Item: z))
                    .Select(x =>
                    {
                        if (x.Item is { } item)
                            return $"{x.constructurArgument.Name} : {item.assignment.Value}";

                        // TODO might be worth considering to throw exception in the source generator for this in order to give faster feedback.
                        return
                            $@"{x.constructurArgument.Name} : true ? throw new ArgumentException(""Unable to determine the corresponding data member for constructor argument '{x.constructurArgument.Name}' in '{namedTypeSymbol.Name}'; make sure the names are consistent."") : default";
                    });

                constructorArgs = string.Join(", ", ctorInitialization);
            }

            var objInitialization = assignments
                .Where(x => x.DDB.DataMember.IsAssignable)
                .Select(x => $"{x.DDB.DataMember.Name} = {x.Assignment.Value}");

            return (
                assignments.Select(x => x.Assignment),
                $"new {_fullTypeNameFactory(typeSymbol)}({constructorArgs}) {{{string.Join(", ", objInitialization)}}}"
            );
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