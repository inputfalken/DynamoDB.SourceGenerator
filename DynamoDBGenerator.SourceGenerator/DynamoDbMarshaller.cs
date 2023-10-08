using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace DynamoDBGenerator.SourceGenerator;

public class DynamoDbMarshaller
{
    private const string DeserializeName = "Unmarshall";
    private const string InterfaceName = "IDynamoDBMarshaller";
    private const string NameTrackerName = "AttributeNameExpressionTracker";
    private const string SerializeName = "Marshall";
    private const string ValueTrackerName = "AttributeExpressionValueTracker";
    private readonly DynamoDBKeyStructure? _keyStructure;
    private readonly DynamoDBMarshallerArguments _arguments;
    private readonly Func<ITypeSymbol, DynamoDbDataMember[]> _cachedDataMembers;
    private readonly Func<ITypeSymbol, KnownType?> _knownTypeFactory;
    private readonly Func<ITypeSymbol, string> _attributeNameAssignmentNameFactory;
    private readonly Func<ITypeSymbol, string> _attributeValueAssignmentNameFactory;
    private readonly Func<ITypeSymbol, string> _deserializationMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _fullTypeNameFactory;
    private readonly Func<ITypeSymbol, string> _keysMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _serializationMethodNameFactory;
    private readonly IEqualityComparer<ITypeSymbol> _comparer;

    public DynamoDbMarshaller(in DynamoDBMarshallerArguments arguments)
    {
        _arguments = arguments;
        _attributeNameAssignmentNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("Names", _arguments.SymbolComparer, false);
        _attributeValueAssignmentNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("Values", _arguments.SymbolComparer, false);
        _cachedDataMembers = TypeExtensions.CacheFactory(_arguments.SymbolComparer, static x => x.GetDynamoDbProperties().ToArray());
        _comparer = _arguments.SymbolComparer;
        _deserializationMethodNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("_U", _arguments.SymbolComparer, true);
        _fullTypeNameFactory = TypeExtensions.CacheFactory(_arguments.SymbolComparer, static x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        _keysMethodNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("_K", _arguments.SymbolComparer, false);
        _keyStructure = _cachedDataMembers(arguments.EntityTypeSymbol).GetKeyStructure();
        _knownTypeFactory = TypeExtensions.CacheFactory(_arguments.SymbolComparer, static x => x.GetKnownType());
        _serializationMethodNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("_M", _arguments.SymbolComparer, true);
    }
    private Assignment AttributeValueAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        if (_knownTypeFactory(typeSymbol) is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

        var assignment = knownType switch
        {
            BaseType baseType => baseType.Type switch
            {
                BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"S = {accessPattern}", knownType),
                BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"BOOL = {accessPattern}", knownType),
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
                    => typeSymbol.ToInlineAssignment($"N = {accessPattern}.ToString()", knownType),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"S = {accessPattern}.ToString()", knownType),
                BaseType.SupportedType.DateOnly or BaseType.SupportedType.DateTimeOffset or BaseType.SupportedType.DateTime => typeSymbol.ToInlineAssignment($@"S = {accessPattern}.ToString(""O"")", knownType),
                BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"N = ((int){accessPattern}).ToString()", knownType),
                BaseType.SupportedType.MemoryStream => typeSymbol.ToInlineAssignment($"B = {accessPattern}", knownType),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => AttributeValueAssignment(singleGeneric.T, $"{accessPattern}.Value"),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.Array
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => BuildList(singleGeneric.T, in accessPattern),
                SingleGeneric.SupportedType.Set => BuildSet(singleGeneric.T, accessPattern, knownType),
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

        return new Assignment(in outerAssignment, in elementType, innerAssignment.KnownType);
    }

    private Assignment BuildPocoList(in SingleGeneric singleGeneric, in string? operation, in string accessPattern, in string defaultCause, in string memberName)
    {
        var innerAssignment = DataMemberAssignment(singleGeneric.T, "y", in memberName);
        var outerAssignment = $"{accessPattern} switch {{ {{ L: {{ }} x }} => x.Select(y => {innerAssignment.Value}){operation}, {defaultCause} }}";

        return new Assignment(in outerAssignment, singleGeneric.T, innerAssignment.KnownType);
    }

    private static Assignment? BuildPocoSet(in ITypeSymbol elementType, in string accessPattern, in string defaultCase, KnownType knownType)
    {
        if (elementType.IsNumeric())
            return elementType.ToInlineAssignment($"{accessPattern} switch {{ {{ NS: {{ }} x }} =>  new HashSet<{elementType.Name}>(x.Select(y => {elementType.Name}.Parse(y))), {defaultCase} }}", knownType);

        if (elementType.SpecialType is SpecialType.System_String)
            return elementType.ToInlineAssignment($"{accessPattern} switch {{ {{ SS: {{ }} x }} =>  new HashSet<string>(x), {defaultCase} }}", knownType);

        return null;
    }

    private static Assignment? BuildSet(in ITypeSymbol elementType, in string accessPattern, KnownType knownType)
    {
        var newAccessPattern = elementType.NotNullLambdaExpression() is { } expression
            ? $"{accessPattern}.Where({expression})"
            : accessPattern;

        if (elementType.SpecialType is SpecialType.System_String)
            return elementType.ToInlineAssignment($"SS = new List<string>({newAccessPattern})", knownType);

        return elementType.IsNumeric() is false
            ? null
            : elementType.ToInlineAssignment($"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))", knownType);
    }

    private string CreateAttributePocoFactory()
    {
        var enumerable = Conversion.ConversionMethods(
                _arguments.EntityTypeSymbol,
                StaticPocoFactory,
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Select(static x => x.Code);

        return string.Join(Constants.NewLine, enumerable);
    }

    private string CreateAttributeValueFactory()
    {
        var hashset = new HashSet<ITypeSymbol>(_comparer);
        var entityTypeSymbol = _arguments.EntityTypeSymbol;
        var code = Conversion.ConversionMethods(
            entityTypeSymbol,
            StaticAttributeValueDictionaryFactory,
            hashset
        );

        var argumentTypeSymbol = _arguments.ArgumentType;
        return string.Join(
            Constants.NewLine,
            (_comparer.Equals(entityTypeSymbol, argumentTypeSymbol)
                ? code
                : code.Concat(Conversion.ConversionMethods(argumentTypeSymbol, StaticAttributeValueDictionaryFactory, hashset))
            ).Select(x => x.Code)
        );

    }


    public string CreateDynamoDbDocumentProperty(Accessibility accessibility)
    {
        var keysMethod = StaticAttributeValueDictionaryKeys(_keyStructure, "partitionKey", "rangeKey", "isPartitionKey", "isRangeKey");
        var entityTypeSymbol = _arguments.EntityTypeSymbol;
        var rootTypeName = _fullTypeNameFactory(entityTypeSymbol);
        var argumentTypeSymbol = _arguments.ArgumentType;
        var valueTrackerTypeName = _attributeValueAssignmentNameFactory(argumentTypeSymbol);
        var nameTrackerTypeName = _attributeNameAssignmentNameFactory(entityTypeSymbol);

        var implementInterface =
            $@"public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {SerializeName}({rootTypeName} entity) => {_serializationMethodNameFactory(entityTypeSymbol)}(entity);
            public {rootTypeName} {DeserializeName}({nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> entity) => {_deserializationMethodNameFactory(entityTypeSymbol)}(entity);
            public {Constants.KeyMarshallerInterFaceName} PrimaryKeyMarshaller {{ get; }} = new {Constants.KeyMarshallerImplementationTypeName}({_keysMethodNameFactory(entityTypeSymbol)});
            public {Constants.IndexKeyMarshallerInterfaceName} IndexKeyMarshaller(string index) => new {Constants.IndexKeyMarshallerImplementationTypeName}({_keysMethodNameFactory(entityTypeSymbol)}, index);
            public {_arguments.ImplementationName}.{valueTrackerTypeName} {ValueTrackerName}()
            {{
                var number = 0;
                Func<string> valueIdProvider = () => $"":p{{++number}}"";
                return new {_arguments.ImplementationName}.{valueTrackerTypeName}(valueIdProvider);
            }}
            public {_arguments.ImplementationName}.{nameTrackerTypeName} {NameTrackerName}()
            {{
                return new {_arguments.ImplementationName}.{nameTrackerTypeName}(null);
            }}
{CreateAttributeValueFactory()}
{keysMethod}
{CreateAttributePocoFactory()}";

        var sourceGeneratedCode = string.Join(Constants.NewLine, CreateExpressionAttributeTrackers().Prepend(implementInterface));

        var @interface =
            $"{InterfaceName}<{rootTypeName}, {_fullTypeNameFactory(argumentTypeSymbol)}, {_arguments.ImplementationName}.{nameTrackerTypeName}, {_arguments.ImplementationName}.{valueTrackerTypeName}>";
        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{_arguments.ImplementationName}: {@interface}",
            in sourceGeneratedCode,
            2
        );

        return
            $@"{accessibility.ToCode()} static {@interface} {_arguments.ConsumerAccessProperty} {{ get; }} = new {_arguments.ImplementationName}();
        {@class}";
    }
    private IEnumerable<string> CreateExpressionAttributeTrackers()
    {
        return Conversion.ConversionMethods(
                _arguments.EntityTypeSymbol,
                ExpressionAttributeName,
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Concat(Conversion.ConversionMethods(
                    _arguments.ArgumentType,
                    ExpressionAttributeValue,
                    new HashSet<ITypeSymbol>(_comparer)
                )
            )
            .Select(static x => x.Code);
    }

    private Assignment DataMemberAssignment(in ITypeSymbol type, in string pattern, in string memberName)
    {

        var defaultCase = type.IsNullable() ? "_ => null" : @$"_ => throw {Constants.NullExceptionMethod}(""{memberName}"")";
        return Execution(in type, in pattern, defaultCase, in memberName);

        Assignment Execution(in ITypeSymbol typeSymbol, in string accessPattern, string @default, in string memberName)
        {
            if (_knownTypeFactory(typeSymbol) is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

            var assignment = knownType switch
            {
                BaseType baseType => baseType.Type switch
                {
                    BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x, {@default} }}", knownType),
                    BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ BOOL: var x }} => x, {@default} }}", knownType),
                    BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x[0], {@default} }}", knownType),
                    BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} when Int32.Parse(x) is var y =>({_fullTypeNameFactory(typeSymbol)})y, {@default} }}", knownType),
                    BaseType.SupportedType.Int16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int16.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Byte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Byte.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Int32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int32.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Int64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int64.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.SByte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => SByte.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.UInt16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt16.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.UInt32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt32.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.UInt64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{  {{ N: {{ }} x }} => UInt64.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Decimal => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Decimal.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Double => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Double.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Single => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Single.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.DateTime => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTime.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.DateTimeOffset => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTimeOffset.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.DateOnly => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateOnly.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.MemoryStream => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ B: {{ }} x }} => x, {@default} }}", knownType),
                    _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
                },
                SingleGeneric singleGeneric => singleGeneric.Type switch
                {
                    SingleGeneric.SupportedType.Nullable => Execution(singleGeneric.T, in accessPattern, @default, in memberName),
                    SingleGeneric.SupportedType.Set => BuildPocoSet(singleGeneric.T, in accessPattern, in @default, knownType),
                    SingleGeneric.SupportedType.Array => BuildPocoList(in singleGeneric, ".ToArray()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.ICollection => BuildPocoList(in singleGeneric, ".ToList()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.IReadOnlyCollection => BuildPocoList(in singleGeneric, ".ToArray()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.IEnumerable => BuildPocoList(in singleGeneric, null, in accessPattern, in @default, in memberName),
                    _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
                },
                KeyValueGeneric keyValueGeneric => StringKeyedPocoGeneric(in keyValueGeneric, in accessPattern, in @default, in memberName),
                _ => null
            };

            return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

            Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
                typeSymbol.ToExternalDependencyAssignment($"{accessPattern} switch {{ {{ M: {{ }} x }} => {_deserializationMethodNameFactory(typeSymbol)}(x), {@default} }}");
        }

    }
    private Conversion ExpressionAttributeName(ITypeSymbol typeSymbol)
    {
        var dataMembers = _cachedDataMembers(typeSymbol)
            .Where(static x => x.IsIgnored is false)
            .Select(x => (
                KnownType: _knownTypeFactory(x.DataMember.Type),
                DDB: x,
                NameRef: $"_{x.DataMember.Name}NameRef",
                AttributeReference: _attributeNameAssignmentNameFactory(x.DataMember.Type),
                AttributeInterfaceName: AttributeInterfaceName()))
            .ToArray();

        var fieldDeclarations = dataMembers
            .Select(static x => x.KnownType is not null
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
                var assignment = x.KnownType is not null
                    ? $"        {x.NameRef} = new (() => {ternaryExpressionName});"
                    : $"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ternaryExpressionName}));";

                return new Assignment(assignment, x.DDB.DataMember.Type, x.KnownType);
            })
            .ToArray();

        var className = _attributeNameAssignmentNameFactory(typeSymbol);
        var constructor = $@"public {className}(string? {constructorAttributeName})
    {{
{string.Join(Constants.NewLine, fieldAssignments.Select(x => x.Value))}
    }}";

        var expressionAttributeNameYields = dataMembers.Select(static x => x.KnownType is not null
            ? $@"       if ({x.NameRef}.IsValueCreated) yield return new ({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
            : $"       if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{nameof(IExpressionAttributeNameTracker.AccessedNames)}()) {{ yield return x; }}");

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
        return new Conversion(@class, fieldAssignments);

        string AttributeInterfaceName() => nameof(IExpressionAttributeNameTracker);
    }

    private Conversion ExpressionAttributeValue(ITypeSymbol typeSymbol)
    {
        var dataMembers = _cachedDataMembers(typeSymbol)
            .Where(static x => x.IsIgnored is false)
            .Select(x => (
                KnownType: _knownTypeFactory(x.DataMember.Type),
                DDB: x,
                ValueRef: $"_{x.DataMember.Name}ValueRef",
                AttributeReference: _attributeValueAssignmentNameFactory(x.DataMember.Type),
                AttributeInterfaceName: AttributeInterfaceName(x.DataMember.Type)))
            .ToArray();

        var fieldDeclarations = dataMembers
            .Select(static x => x.KnownType is not null
                ? $@"    private readonly Lazy<string> {x.ValueRef};
    public string {x.DDB.DataMember.Name} => {x.ValueRef}.Value;"
                : $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};
    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;"
            );

        const string valueProvider = "valueIdProvider";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                var assignment = x.KnownType is not null
                    ? $"        {x.ValueRef} = new ({valueProvider});"
                    : $"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({valueProvider}));";

                return new Assignment(assignment, x.DDB.DataMember.Type, x.KnownType);
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
                return x.KnownType is not null
                    ? $"       if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {AttributeValueAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}"
                    : $"       if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{nameof(IExpressionAttributeValueTracker<object>.AccessedValues)}({accessPattern})) {{ yield return x; }}")}";
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
        return new Conversion(@class, fieldAssignments);

        string AttributeInterfaceName(ITypeSymbol symbol) => $"{nameof(IExpressionAttributeValueTracker<object>)}<{_fullTypeNameFactory(symbol)}>";
    }
    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = GetAssignments(type).ToArray();

        const string indent = "                ";
        var method =
            @$"            private static Dictionary<string, AttributeValue> {_serializationMethodNameFactory(type)}({_fullTypeNameFactory(type)} {paramReference})
            {{
                {InitializeDictionary(dictionaryName, properties.Select(static x => x.capacityTernary))}
                {string.Join(Constants.NewLine + indent, properties.Select(static x => x.dictionaryPopulation))}
                return {dictionaryName};
            }}";

        return new Conversion(
            in method,
            properties.Select(static x => x.assignment)
        );

        IEnumerable<(string dictionaryPopulation, string capacityTernary, Assignment assignment)> GetAssignments(ITypeSymbol typeSymbol)
        {
            foreach (var x in _cachedDataMembers(typeSymbol))
            {
                if (x.IsIgnored)
                    continue;

                var accessPattern = $"{paramReference}.{x.DataMember.Name}";
                var attributeValue = AttributeValueAssignment(x.DataMember.Type, accessPattern);

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
                var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: capacity);"
            };
        }
    }
    private string StaticAttributeValueDictionaryKeys(
        DynamoDBKeyStructure? keyStructure,
        string pkReference,
        string rkReference,
        string enforcePkValidation,
        string enforceRkValidation
    )
    {
        const string dictionaryName = "attributeValues";

        string body;
        var entityTypeSymbol = _arguments.EntityTypeSymbol;
        if (keyStructure is null)
            body = @$"throw {Constants.NoDynamoDBKeyAttributesExceptionMethod}(""{entityTypeSymbol.Name}"");";
        else
        {

            var switchCases = GetAssignments(pkReference, rkReference, enforcePkValidation, enforceRkValidation, keyStructure.Value)
                .Select(x => @$"                    case {(x.IndexName is null ? "null" : @$"""{x.IndexName}""")}:
                    {{
{x.assignments}
                        break;
                    }}");

            body = @$"var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: 2);
                switch (index)
                {{
{string.Join(Constants.NewLine, switchCases)}
                    default: 
                        throw {Constants.MissMatchedIndexNameExceptionMethod}(nameof(index), index);
                }}
                var keysCount = {dictionaryName}.Count;
                if ({enforcePkValidation} && {enforceRkValidation} && keysCount == 2)
                    return {dictionaryName};
                if ({enforcePkValidation} && {enforceRkValidation} is false && keysCount == 1)
                    return {dictionaryName};
                if ({enforcePkValidation} is false && {enforceRkValidation} && keysCount == 1)
                    return {dictionaryName};
                if ({enforcePkValidation} && {enforceRkValidation} && keysCount == 1)
                    throw {Constants.KeysMissingDynamoDBAttributeExceptionMethod}({pkReference}, {rkReference});
                throw {Constants.ShouldNeverHappenExceptionMethod}();";
        }

        var method =
            @$"            private static Dictionary<string, AttributeValue> {_keysMethodNameFactory(entityTypeSymbol)}(object? {pkReference}, object? {rkReference}, bool {enforcePkValidation}, bool {enforceRkValidation}, string? index = null)
            {{
                {body}
            }}";

        return method;

        IEnumerable<(string? IndexName, string assignments)> GetAssignments(
            string partition,
            string range,
            string enforcePartition,
            string enforceRange,
            DynamoDBKeyStructure keyStructure
        )
        {
            yield return keyStructure switch
            {
                {PartitionKey: var pk, SortKey: { } sortKey} => (null, $"{CreateAssignment(enforcePartition, partition, pk)}{Constants.NewLine}{CreateAssignment(enforceRange, range, sortKey)}"),
                {PartitionKey: var pk, SortKey: null} => (null, $"{CreateAssignment(enforcePartition, partition, pk)}{Constants.NewLine}{MissingAssigment(enforceRange, range)}")
            };

            foreach (var gsi in keyStructure.GlobalSecondaryIndices)
                yield return gsi switch
                {
                    {PartitionKey: var pk, SortKey: { } sortKey} => (gsi.Name, $"{CreateAssignment(enforcePartition, partition, pk)}{Constants.NewLine}{CreateAssignment(enforceRange, range, sortKey)}"),
                    {PartitionKey: var pk, SortKey: null} => (gsi.Name, $"{CreateAssignment(enforcePartition, partition, pk)}{Constants.NewLine}{MissingAssigment(enforceRange, range)}")
                };

            foreach (var lsi in keyStructure.LocalSecondaryIndices)
                yield return (lsi, keyStructure.PartitionKey) switch
                {
                    {PartitionKey: var pk, lsi: var sortKey} => (lsi.Name, $"{CreateAssignment(enforcePartition, partition, pk)}{Constants.NewLine}{CreateAssignment(enforceRange, range, sortKey.SortKey)}")
                };

            string MissingAssigment(string validateReference, string keyReference)
            {
                var expression = $"{validateReference} && {keyReference} is not null";
                return $@"                        if({expression}) 
                            throw {Constants.KeysValueWithNoCorrespondenceMethod}(""{keyReference}"", {keyReference});";
            }

            string CreateAssignment(string validateReference, string keyReference, DynamoDbDataMember dataMember)
            {
                const string reference = "value";
                var attributeConversion = AttributeValueAssignment(dataMember.DataMember.Type, reference);
                var expectedType = _fullTypeNameFactory(dataMember.DataMember.Type);
                var expression = $"{keyReference} is {expectedType} {{ }} {reference}";

                return $@"                        if({validateReference}) 
                        {{ 
                            if ({expression}) 
                                {dictionaryName}.Add(""{dataMember.AttributeName}"", {attributeConversion.ToAttributeValue()});
                            else if ({keyReference} is null) 
                                throw {Constants.KeysArgumentNullExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"");
                            else 
                                throw {Constants.KeysInvalidConversionExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"", {keyReference}, ""{expectedType}"");
                        }}";
            }

        }

    }

    private Conversion StaticPocoFactory(ITypeSymbol type)
    {

        var values = GetAssignments(type);
        const string paramReference = "entity";
        var method =
            @$"            private static {_fullTypeNameFactory(type)} {_deserializationMethodNameFactory(type)}(Dictionary<string, AttributeValue> {paramReference})
            {{ 
                return {values.objectInitialization};
            }}";

        return new Conversion(method, values.Item1);

        (IEnumerable<Assignment>, string objectInitialization) GetAssignments(ITypeSymbol typeSymbol)
        {
            var assignments = _cachedDataMembers(typeSymbol)
                .Select(x => (DDB: x, Assignment: DataMemberAssignment(x.DataMember.Type, @$"{paramReference}.GetValueOrDefault(""{x.AttributeName}"")", x.DataMember.Name)))
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
{string.Join($",{Constants.NewLine}", y.Where(z => z.DDB.DataMember.IsAssignable).Select(z => $"                    {z.DDB.DataMember.Name} = {z.Assignment.Value}"))}
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

    private Assignment? StringKeyedPocoGeneric(in KeyValueGeneric keyValueGeneric, in string accessPattern, in string defaultCase, in string memberName)
    {
        switch (keyValueGeneric)
        {
            case {TKey: not {SpecialType: SpecialType.System_String}}:
                return null;
            case {Type: KeyValueGeneric.SupportedType.LookUp}:
                var lookupValueAssignment = DataMemberAssignment(keyValueGeneric.TValue, "y.z", in memberName);
                return new Assignment(
                    $"{accessPattern} switch {{ {{ M: {{ }} x }} => x.SelectMany(y => y.Value.L, (y, z) => (y.Key, z)).ToLookup(y => y.Key, y => {lookupValueAssignment.Value}), {defaultCase} }}",
                    keyValueGeneric.TValue,
                    lookupValueAssignment.KnownType
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = DataMemberAssignment(keyValueGeneric.TValue, "y.Value", in memberName);
                return new Assignment(
                    $"{accessPattern} switch {{ {{ M: {{ }} x }} => x.ToDictionary(y => y.Key, y => {dictionaryValueAssignment.Value}), {defaultCase} }}",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.KnownType
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
                    lookupValueAssignment.KnownType
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = AttributeValueAssignment(keyValueGeneric.TValue, "x.Value");
                return new Assignment(
                    $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {dictionaryValueAssignment.ToAttributeValue()})",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.KnownType
                );
            default:
                return null;
        }
    }
}