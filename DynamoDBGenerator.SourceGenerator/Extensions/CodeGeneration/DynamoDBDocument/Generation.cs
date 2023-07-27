using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.DynamoDBDocument;

public class DynamoDbDocumentGenerator
{
    private const string DeserializeName = "Deserialize";
    private const string KeysName = "Keys";
    private const string SerializeName = "Serialize";
    private const string ReferenceTrackerName = "ExpressionAttributeTracker";
    private const string DynamoDbDocumentName = "IDynamoDBDocument";

    private readonly INamedTypeSymbol _rootTypeSymbol;
    private readonly IEqualityComparer<ITypeSymbol> _comparer;
    private readonly Func<ITypeSymbol, string> _attributeValueFactoryMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _pocoMethodNameFactory;
    private readonly Func<ITypeSymbol, string> _createTypeName;
    private readonly string _rootTypeName;

    public DynamoDbDocumentGenerator(in INamedTypeSymbol typeSymbol, IEqualityComparer<ITypeSymbol> comparer)
    {
        _rootTypeSymbol = typeSymbol;
        _attributeValueFactoryMethodNameFactory = TypeExtensions.CachedTypeStringificationFactory("Factory", comparer);
        _pocoMethodNameFactory = TypeExtensions.CachedTypeStringificationFactory("POCO", comparer);
        _createTypeName = TypeExtensions.CachedTypeStringificationFactory("References", comparer);
        _comparer = comparer;
        _rootTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private Assignment DataMemberAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        var defaultCause = typeSymbol.IsNullable() ? "_ => null" : "_ => throw new ArgumentNullException()";
        if (typeSymbol.GetKnownType() is not { } knownType) return ExternalAssignment(typeSymbol, accessPattern, defaultCause);

        Assignment? assignment = knownType switch
        {
            BaseType baseType => baseType.Type switch
            {
                BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: var x }} => x, {defaultCause} }}"),
                BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ BOOL: var x }} => x, {defaultCause} }}"),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: var x }} => x[0], {defaultCause} }}"),
                BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} when Int32.Parse(x) is var y =>({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})y, {defaultCause} }}"),
                BaseType.SupportedType.System_Int16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Int16.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_Byte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Byte.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_Int32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Int32.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_Int64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Int64.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_SByte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => SByte.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_UInt16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => UInt16.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_UInt32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => UInt32.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_UInt64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{  {{ N: var x }} => UInt64.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_Decimal => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Decimal.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_Double => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Double.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_Single => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: var x }} => Single.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_DateTime => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: var x }} => DateTime.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_DateTimeOffset => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: var x }} => DateTimeOffset.Parse(x), {defaultCause} }}"),
                BaseType.SupportedType.System_DateOnly => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: var x }} => DateOnly.Parse(x), {defaultCause} }}"),
                _ => throw new ArgumentOutOfRangeException()
            },
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => DataMemberAssignment(singleGeneric.T, accessPattern),

                _ => null,
//                SingleGeneric.SupportedType.Set => expr,
//                SingleGeneric.SupportedType.Collection => expr,
//                _ => throw new ArgumentOutOfRangeException()
            },
            _ => null
        };

        return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern, in defaultCause);

        Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern, in string defaultCause) =>
            typeSymbol.ToExternalDependencyAssignment($"{accessPattern} switch {{ {{ M: var x }} => {_pocoMethodNameFactory(typeSymbol)}(x), {defaultCause} }}");
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
                BaseType.SupportedType.System_Int16
                    or BaseType.SupportedType.System_Int32
                    or BaseType.SupportedType.System_Int64
                    or BaseType.SupportedType.System_UInt16
                    or BaseType.SupportedType.System_UInt32
                    or BaseType.SupportedType.System_UInt64
                    or BaseType.SupportedType.System_Double
                    or BaseType.SupportedType.System_Decimal
                    or BaseType.SupportedType.System_Single
                    or BaseType.SupportedType.System_SByte
                    or BaseType.SupportedType.System_Byte
                    => typeSymbol.ToInlineAssignment($"N = {accessPattern}.ToString()"),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"S = {accessPattern}.ToString()"),
                BaseType.SupportedType.System_DateOnly or BaseType.SupportedType.System_DateTimeOffset or BaseType.SupportedType.System_DateTime => typeSymbol.ToInlineAssignment($@"S = {accessPattern}.ToString(""O"")"),
                BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"N = ((int){accessPattern}).ToString()"),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Collection => BuildList(singleGeneric.T, in accessPattern),
                SingleGeneric.SupportedType.Nullable => AttributeValueAssignment(singleGeneric.T, $"{accessPattern}.Value"),
                SingleGeneric.SupportedType.Set => BuildSet(singleGeneric.T, accessPattern),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            KeyValueGeneric keyValueGeneric => StringKeyedValuedGeneric(in keyValueGeneric, in accessPattern),
            _ => null
        };

        return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

        Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
            typeSymbol.ToExternalDependencyAssignment($"M = {_attributeValueFactoryMethodNameFactory(typeSymbol)}({accessPattern})");
    }

    private Assignment BuildList(in ITypeSymbol elementType, in string accessPattern)
    {
        var attributeValue = AttributeValueAssignment(elementType, "x");
        var select = $"Select(x => {attributeValue.ToAttributeValue()}))";
        var assignment = elementType.NotNullLambdaExpression() is { } whereBody
            ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
            : $"L = new List<AttributeValue>({accessPattern}.{select}";

        return new Assignment(in assignment, in elementType, attributeValue.HasExternalDependency);
    }

    private static Assignment? BuildSet(in ITypeSymbol elementType, in string accessPattern)
    {
        var newAccessPattern = elementType.NotNullLambdaExpression() is { } expression
            ? $"{accessPattern}.Where({expression})"
            : accessPattern;

        if (elementType.SpecialType is SpecialType.System_String)
            return new Assignment(
                $"SS = new List<string>({newAccessPattern})",
                in elementType,
                false
            );

        return elementType.IsNumeric() is false
            ? null
            : new Assignment(
                $"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))",
                in elementType,
                false
            );
    }

    private string CreateAttributeValueFactory(KeyStrategy keyStrategy)
    {
        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                x => StaticAttributeValueDictionaryFactory(x, keyStrategy),
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Select(static x => x.Code);

        return string.Join(Constants.NewLine, enumerable);

    }
    private string CreateAttributePocoFactory()
    {
        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                StaticPocoFactory,
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Select(static x => x.Code);

        return string.Join(Constants.NewLine, enumerable);

    }


    public string CreateDynamoDbDocumentProperty(Accessibility accessibility)
    {
        var referenceTrackers = Conversion.ConversionMethods(
                _rootTypeSymbol,
                ExpressionAttributeReferencesClassGenerator,
                new HashSet<ITypeSymbol>(_comparer)
            )
            .Select(static x => x.Code);

        var className = $"{_rootTypeSymbol.Name}_Document";
        var marshalMethods = CreateAttributeValueFactory(KeyStrategy.Include);
        var keysMethod = CreateAttributeValueFactory(KeyStrategy.Only);
        var keysClass = CodeGenerationExtensions.CreateClass(Accessibility.Private, "KeysClass", keysMethod, 2);
        var unMarshalMethods = CreateAttributePocoFactory();

        var expressionAttributeName = _createTypeName(_rootTypeSymbol);
        var implementInterface =
            $@"public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {SerializeName}({_rootTypeName} entity) => {_attributeValueFactoryMethodNameFactory(_rootTypeSymbol)}(entity);
            public {_rootTypeName} {DeserializeName}({nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> entity) => {_pocoMethodNameFactory(_rootTypeSymbol)}(entity);
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {KeysName}({_rootTypeName} entity) => KeysClass.{_attributeValueFactoryMethodNameFactory(_rootTypeSymbol)}(entity);
            public {className}.{expressionAttributeName} {ReferenceTrackerName}()
            {{
                var number = 0;
                Func<string> valueIdProvider = () => $"":p{{++number}}"";
                return new {className}.{expressionAttributeName}(null, valueIdProvider);
            }}
{marshalMethods}
{keysClass}
{unMarshalMethods}";

        var sourceGeneratedCode = string.Join(Constants.NewLine, referenceTrackers.Prepend(implementInterface));

        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className}: {DynamoDbDocumentName}<{_rootTypeSymbol.Name}, {className}.{expressionAttributeName}>",
            in sourceGeneratedCode,
            2
        );

        var propertyName = $"{_rootTypeSymbol.Name}Document";
        return ($@"{accessibility.ToCode()} {DynamoDbDocumentName}<{_rootTypeName}, {className}.{expressionAttributeName}> {propertyName} {{ get; }} = new {className}();
{@class}");
    }

    private Conversion ExpressionAttributeReferencesClassGenerator(ITypeSymbol typeSymbol)
    {
        const string accessedValues = nameof(IExpressionAttributeReferences<int>.AccessedValues);
        const string accessedNames = nameof(IExpressionAttributeReferences<int>.AccessedNames);

        var dataMembers = typeSymbol
            .GetDynamoDbProperties()
            .Where(static x => x.IsIgnored is false)
            .Select(x => (
                IsKnown: x.DataMember.Type.GetKnownType() is not null,
                DDB: x,
                NameRef: $"_{x.DataMember.Name}NameRef",
                ValueRef: $"_{x.DataMember.Name}ValueRef",
                AttributeReference: _createTypeName(x.DataMember.Type),
                AttributeInterfaceName: AttributeInterfaceName(x.DataMember.Type)))
            .ToArray();

        var fieldDeclarations = dataMembers
            .Select(static x =>
            {
                if (x.IsKnown)
                {
                    return $@"    private readonly Lazy<string> {x.ValueRef};
    private readonly Lazy<string> {x.NameRef};
    public AttributeReference {x.DDB.DataMember.Name} {{ get; }}";
                }
                return $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};
    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;";
            });

        const string constructorAttributeName = "nameRef";
        const string valueProvider = "valueIdProvider";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                string assignment;
                var ternaryExpressionName =
                    $"{constructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                if (x.IsKnown)
                {
                    assignment =
                        $@"        {x.ValueRef} = new ({valueProvider});
        {x.NameRef} = new (() => {ternaryExpressionName});
        {x.DDB.DataMember.Name} = new (in {x.NameRef}, in {x.ValueRef});";
                }
                else
                {
                    assignment =
                        $@"        _{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ternaryExpressionName}, {valueProvider}));";
                }

                return new Assignment(assignment, x.DDB.DataMember.Type, x.IsKnown is false);
            })
            .ToArray();

        var className = _createTypeName(typeSymbol);
        var constructor = $@"public {className}(string? {constructorAttributeName}, Func<string> {valueProvider})
    {{
{string.Join(Constants.NewLine, fieldAssignments.Select(x => x.Value))}
    }}";

        var expressionAttributeNameYields = dataMembers.Select(static x => x.IsKnown
            ? $@"       if ({x.NameRef}.IsValueCreated) yield return new ({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
            : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{accessedNames}()) {{ yield return x; }}");
        var expressionAttributeValueYields = dataMembers
            .Select(x =>
            {
                var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                return x.IsKnown
                    ? $@"       if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {AttributeValueAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}"
                    : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{accessedValues}({accessPattern})) {{ yield return x; }}")}";
            });

        var interfaceName = AttributeInterfaceName(typeSymbol);
        var @class = CodeGenerationExtensions.CreateStruct(
            Accessibility.Public,
            $"{className} : {interfaceName}",
            $@"{constructor}
{string.Join(Constants.NewLine, fieldDeclarations)}
    IEnumerable<KeyValuePair<string, string>> {interfaceName}.{accessedNames}()
    {{
{(string.Join(Constants.NewLine, expressionAttributeNameYields) is var joinedNames && joinedNames != string.Empty ? joinedNames : "return Enumerable.Empty<KeyValuePair<string, string>>();")}
    }}
    IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{accessedValues}({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} entity)
    {{
{(string.Join(Constants.NewLine, expressionAttributeValueYields) is var joinedValues && joinedValues != string.Empty ? joinedValues : "return Enumerable.Empty<KeyValuePair<string, AttributeValue>>();")}
    }}",
            0,
            isReadonly: true,
            isRecord: true
        );
        return new Conversion(@class, fieldAssignments.Where(x => x.HasExternalDependency));

        static string AttributeInterfaceName(ITypeSymbol typeSymbol) => $"{nameof(IExpressionAttributeReferences<object>)}<{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>";
    }

    private Conversion StaticPocoFactory(ITypeSymbol type)
    {

        var assignments = GetAssignments(type).ToArray();
        const string elementName = "element";
        var declaration = $"var {elementName} = new {type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()";
        const string paramReference = "entity";
        var method =
            @$"            public static {type.ToDisplayString(NullableFlowState.None, SymbolDisplayFormat.FullyQualifiedFormat)} {_pocoMethodNameFactory(type)}(Dictionary<string, AttributeValue> {paramReference})
            {{ 
                {declaration};
{string.Join(Constants.NewLine, assignments.Select(x => x.pocoAssignment))}
                return {elementName};
            }}";

        return new Conversion(method, assignments.Select(x => x.assignment).Where(x => x.HasExternalDependency));

        IEnumerable<(string pocoAssignment, Assignment assignment)> GetAssignments(
            INamespaceOrTypeSymbol typeSymbol
        )
        {
            foreach (var x in typeSymbol.GetDynamoDbProperties())
            {
                if (x.IsIgnored)
                    continue;

                var accessPattern = @$"{paramReference}.GetValueOrDefault(""{x.AttributeName}"")";

                var assignment = DataMemberAssignment(x.DataMember.Type, accessPattern);
                var dictionaryAssignment = @$"                {elementName}.{x.DataMember.Name} = {assignment.Value};";

                yield return (dictionaryAssignment, assignment);

            }
        }

    }
    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type, KeyStrategy keyStrategy)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = GetAssignments(type, keyStrategy).ToArray();

        const string indent = "                ";
        var method =
            @$"            public static Dictionary<string, AttributeValue> {_attributeValueFactoryMethodNameFactory(type)}({type.ToDisplayString(NullableFlowState.None, SymbolDisplayFormat.FullyQualifiedFormat)} {paramReference})
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
            case {Type: KeyValueGeneric.SupportedType.Grouping}:
                var groupingValueAssignment = BuildList(keyValueGeneric.TValue, accessPattern);
                return new Assignment(
                    $@"M = new Dictionary<string, AttributeValue>{{ {{ {accessPattern}.Key, {groupingValueAssignment.ToAttributeValue()}}} }}",
                    keyValueGeneric.TValue,
                    groupingValueAssignment.HasExternalDependency
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