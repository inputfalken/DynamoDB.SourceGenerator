using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public class DynamoDbDocumentGenerator
{


    private readonly ITypeSymbol _rootTypeSymbol;
    private readonly Func<ITypeSymbol, string> _createMethodName = TypeExtensions.CachedTypeStringificationFactory("Factory");
    private readonly Func<ITypeSymbol, string> _createTypeName = TypeExtensions.CachedTypeStringificationFactory("References");

    public DynamoDbDocumentGenerator(in ITypeSymbol typeSymbol)
    {
        _rootTypeSymbol = typeSymbol;
    }

    private static string AttributeInterfaceName(ITypeSymbol typeSymbol)
    {
        return $"{nameof(IExpressionAttributeReferences<object>)}<{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>";
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
                BaseType.SupportedType.Number => typeSymbol.ToInlineAssignment($"N = {accessPattern}.ToString()"),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"S = {accessPattern}.ToString()"),
                BaseType.SupportedType.Temporal => typeSymbol.ToInlineAssignment(
                    $@"S = {accessPattern}.ToString(""O"")"),
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
            typeSymbol.ToExternalDependencyAssignment($"M = {_createMethodName(typeSymbol)}({accessPattern})");
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

    /// <summary>
    ///     Creates an Dictionary with string as key and AttributeValue as value.
    /// </summary>
    private (string Code, string MethodName) CreateAttributeValueFactory(
        in MethodConfiguration methodConfiguration,
        KeyStrategy keyStrategy)
    {
        var consumerMethod = CreateMethod(
            _rootTypeSymbol,
            "Dictionary<string, AttributeValue>",
            _createMethodName(_rootTypeSymbol),
            methodConfiguration
        );

        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                x => StaticAttributeValueDictionaryFactory(x, keyStrategy),
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability)
            )
            .Select(static x => x.Code);

        var sourceGeneratedCode = string.Join(Constants.NewLine, enumerable);

        var code = $@"{consumerMethod}
{sourceGeneratedCode}";
        return (code, methodConfiguration.Name);

        static string CreateMethod(
            ITypeSymbol typeSymbol,
            string returnType,
            string methodName,
            MethodConfiguration config)
        {
            var accessModifier = config.AccessModifier.ToCode();
            var signature = config.MethodParameterization switch
            {
                MethodConfiguration.Parameterization.UnparameterizedInstance =>
                    $"{accessModifier} {returnType} {config.Name}() => {methodName}(this);",
                MethodConfiguration.Parameterization.ParameterizedStatic =>
                    $"{accessModifier} static {returnType} {config.Name}({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} item) => {methodName}(item);",
                MethodConfiguration.Parameterization.ParameterizedInstance =>
                    $"{accessModifier} {returnType} {config.Name}({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} item) => {methodName}(item);",
                _ => throw new NotSupportedException($"Config of '{config.MethodParameterization}'.")
            };

            return $@"            {signature}";
        }
    }

    private (string Code, string PropertyAccess) CreateDynamoDbDocumentProperty(Accessibility accessibility)
    {
        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                ExpressionAttributeReferencesClassGenerator,
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default)
            )
            .Select(static x => x.Code);

        var className = $"{_rootTypeSymbol.Name}_Document";
        var marshalMethods = CreateAttributeValueFactory(new MethodConfiguration($"{_rootTypeSymbol.Name}Values")
        {
            AccessModifier = Accessibility.Private,
            MethodParameterization = MethodConfiguration.Parameterization.ParameterizedInstance
        }, KeyStrategy.Include);

        var keysMethod = CreateAttributeValueFactory(new MethodConfiguration($"{_rootTypeSymbol.Name}Keys")
        {
            AccessModifier = Accessibility.Public,
            MethodParameterization = MethodConfiguration.Parameterization.ParameterizedStatic
        }, KeyStrategy.Only);

        var keysClass = CodeGenerationExtensions.CreateClass(Accessibility.Private, "KeysClass", keysMethod.Code, 2);

        var fullyQualifiedName = _rootTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var expressionAttributeName = CreateExpressionAttributeNamesClass(_rootTypeSymbol);
        var implementInterface = $@"
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {nameof(IDynamoDbDocument<object, object>.Serialize)}({fullyQualifiedName} entity) => {marshalMethods.MethodName}(entity);
            public {fullyQualifiedName} {nameof(IDynamoDbDocument<object, object>.Deserialize)}({nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> entity) => throw new NotImplementedException();
            public {nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {nameof(IDynamoDbDocument<object, object>.Keys)}({fullyQualifiedName} entity) => KeysClass.{keysMethod.MethodName}(entity);
            public {nameof(AttributeExpression<object>)}<{fullyQualifiedName}> {nameof(IDynamoDbDocument<object, object>.UpdateExpression)}(Func<{expressionAttributeName}, string> selector)
            {{
                var reference = new {className}.{expressionAttributeName}(null, 0);
                return new {nameof(AttributeExpression<object>)}<{_rootTypeSymbol.Name}>(reference, selector(reference));
            }}
            public {nameof(AttributeExpression<object>)}<{fullyQualifiedName}> {nameof(IDynamoDbDocument<object, object>.ConditionExpression)}(Func<{expressionAttributeName}, string> selector)
            {{
                var reference = new {className}.{expressionAttributeName}(null, 0);
                return new {nameof(AttributeExpression<object>)}<{fullyQualifiedName}>(reference, selector(reference));
            }}
{marshalMethods.Code}
{keysClass}
";

        var sourceGeneratedCode = string.Join(Constants.NewLine, enumerable.Prepend(implementInterface));

        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className}: {nameof(IDynamoDbDocument<object, object>)}<{_rootTypeSymbol.Name}, {className}.{expressionAttributeName}>",
            in sourceGeneratedCode,
            2
        );

        var propertyName = $"{_rootTypeSymbol.Name}Document";
        return ($@"{accessibility.ToCode()} {nameof(IDynamoDbDocument<object, object>)}<{fullyQualifiedName}, {className}.{expressionAttributeName}> {propertyName} {{ get; }} = new {className}();
{@class}", propertyName);
    }

    private string CreateExpressionAttributeNamesClass(ITypeSymbol typeSymbol)
    {
        return _createTypeName(typeSymbol);
    }


    public string DynamoDbDocumentProperty()
    {
        return CreateDynamoDbDocumentProperty(Accessibility.Public).Code;
    }

    private Conversion ExpressionAttributeReferencesClassGenerator(ITypeSymbol typeSymbol)
    {
        const string valueEnumerableMethodName = nameof(IExpressionAttributeReferences<int>.AccessedValues);
        const string nameEnumerableMethodName = nameof(IExpressionAttributeReferences<int>.AccessedNames);

        var dataMembers = typeSymbol
            .GetDynamoDbProperties()
            .Where(static x => x.IsIgnored is false)
            .Select(x => (
                IsKnown: x.DataMember.Type.GetKnownType() is not null,
                DDB: x,
                NameRef: $"_{x.DataMember.Name}NameRef",
                ValueRef: $"_{x.DataMember.Name}ValueRef",
                CountRef: $"{x.DataMember.Name}ParamCountRef",
                AttributeReference: CreateExpressionAttributeNamesClass(x.DataMember.Type),
                AttributeInterfaceName: AttributeInterfaceName(x.DataMember.Type)))
            .ToArray();

        const string constAttributeReferenceCount = "AttributeReferenceCount";
        var fieldDeclarations = dataMembers
            .SelectMany(static x =>
            {
                if (x.IsKnown)
                    return new[]
                    {
                        $@"    private readonly Lazy<string> {x.ValueRef};",
                        $@"    private readonly Lazy<string> {x.NameRef};",
                        $@"    public AttributeReference {x.DDB.DataMember.Name} {{ get; }}"
                    };

                return new[]
                {
                    $@"    private readonly Lazy<{x.AttributeReference}> _{x.DDB.DataMember.Name};",
                    $@"    public {x.AttributeReference} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;"
                };
            })
            .Prepend($"    public const int {constAttributeReferenceCount} = {dataMembers.Count(x => x.IsKnown)};");

        const string constructorAttributeName = "nameRef";
        const string constructorParamCounter = "paramCount";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                string assignment;
                var ternaryExpressionName =
                    $"{constructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                if (x.IsKnown)
                {
                    assignment =
                        $@"        var {x.CountRef} = {constructorParamCounter} += 1;
        {x.ValueRef} = new Lazy<string>(() => $"":p{{{x.CountRef}}}"");
        {x.NameRef} = new Lazy<string>(() => {ternaryExpressionName});
        {x.DDB.DataMember.Name} = new AttributeReference(in {x.NameRef}, in {x.ValueRef});
        ";
                }
                else
                {
                    // We're jumping numbers due to performing addition on AttributeReferenceCount and afterwards performing additional additions per field.
                    assignment =
                        $@"        var {x.CountRef} = {constructorParamCounter} + {x.AttributeReference}.{constAttributeReferenceCount};
        _{x.DDB.DataMember.Name} = new Lazy<{x.AttributeReference}>(() => new {x.AttributeReference}({ternaryExpressionName}, {x.CountRef}));"; }

                return new Assignment(assignment, x.DDB.DataMember.Type, x.IsKnown is false);
            })
            .ToArray();

        var className = CreateExpressionAttributeNamesClass(typeSymbol);
        var constructor = $@"public {className}(string? {constructorAttributeName}, int {constructorParamCounter})
    {{
{string.Join(Constants.NewLine, fieldAssignments.Select(x => x.Value))}
    }}";

        var expressionAttributeNameYields = dataMembers.Select(static x => x.IsKnown
            ? $@"       if ({x.NameRef}.IsValueCreated) yield return new KeyValuePair<string, string>({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
            : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{nameEnumerableMethodName}()) {{ yield return x; }}");
        var expressionAttributeValueYields = dataMembers
            .Select(x =>
            {
                var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                return x.IsKnown
                    ? $@"       if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new KeyValuePair<string, AttributeValue>({x.ValueRef}.Value, {AttributeValueAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}"
                    : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{valueEnumerableMethodName}({accessPattern})) {{ yield return x; }}")}";
            });

        var interfaceName = AttributeInterfaceName(typeSymbol);
        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className} : {interfaceName}",
            $@"{constructor}
{string.Join(Constants.NewLine, fieldDeclarations)}
    IEnumerable<KeyValuePair<string, string>> {interfaceName}.{nameEnumerableMethodName}()
    {{
{(string.Join(Constants.NewLine, expressionAttributeNameYields) is var joinedNames && joinedNames != string.Empty ? joinedNames : "return Enumerable.Empty<KeyValuePair<string, string>>();")}
    }}
    IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{valueEnumerableMethodName}({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} entity)
    {{
{(string.Join(Constants.NewLine, expressionAttributeValueYields) is var joinedValues && joinedValues != string.Empty ? joinedValues : "return Enumerable.Empty<KeyValuePair<string, AttributeValue>>();")}
    }}",
            0
        );
        return new Conversion(@class, fieldAssignments.Where(x => x.HasExternalDependency));
    }

    public string ImplementDynamoDbDocument()
    {
        var dynamoDbDocumentProperty = CreateDynamoDbDocumentProperty(Accessibility.Private);

        return $@"public Dictionary<string, AttributeValue> BuildAttributeValues() => {dynamoDbDocumentProperty.PropertyAccess}.{nameof(IDynamoDbDocument<int, int>.Serialize)}(this);
{dynamoDbDocumentProperty.Code}";

    }


    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type, KeyStrategy keyStrategy)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = GetAssignments(type, keyStrategy).ToArray();

        const string indent = "                ";
        var dictionary =
            @$"        
            public static Dictionary<string, AttributeValue> {_createMethodName(type)}({type.ToDisplayString(NullableFlowState.None, SymbolDisplayFormat.FullyQualifiedFormat)} {paramReference})
            {{ 
                {InitializeDictionary(dictionaryName, properties.Select(static x => x.capacityTernary))}
                {string.Join(Constants.NewLine + indent, properties.Select(static x => x.dictionaryPopulation))}
                return {dictionaryName};
            }}";

        return new Conversion(
            in dictionary,
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