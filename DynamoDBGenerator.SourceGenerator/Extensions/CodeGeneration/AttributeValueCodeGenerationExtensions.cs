using System.Collections;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SpecialType;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public static class AttributeValueCodeGenerationExtensions
{
    public static string CreateAttributeValueDictionaryRootMethod(string methodName)
    {
        return $"public Dictionary<string, AttributeValue> {methodName}() => {methodName}(this);";
    }
    public static (
        string dictionary,
        IEnumerable<(AttributeValueInstance attributeValue, DynamoDbDataMember DDB)> results
        ) CreateStaticAttributeValueDictionaryMethod(
            this IEnumerable<DynamoDbDataMember> propertySymbols,
            ITypeSymbol parent,
            string methodName,
            string accessModifier = Constants.AccessModifiers.Private
        )
    {
        const string indent = "            ";
        var paramReference = parent.Name.FirstCharToLower();

        static string InitializeDictionary(string dictionaryName, string paramReference,
            IEnumerable<DataMember> propertySymbols)
        {
            var capacityCalculation = string.Join(
                " + ",
                propertySymbols.Select(x => x.Type.TernaryExpression($"{paramReference}.{x.Name}", "1", "0"))
            );

            const string capacityReference = "capacity";

            var capacityDeclaration = string.IsNullOrWhiteSpace(capacityCalculation)
                ? $"const int {capacityReference} = 0;"
                : $"var {capacityReference} = {capacityCalculation};";

            var ifCheck = $"if (({capacityReference}) is 0) {{ return {dictionaryName}; }}";

            return
                @$"{capacityDeclaration}
            var {dictionaryName} = new Dictionary<string, AttributeValue>({capacityReference});
            {ifCheck}
";
        }

        const string dictionaryName = "attributeValues";
        var properties = propertySymbols
            .Where(x => x.IsIgnored is false)
            .Select(x => (
                    DDB: x,
                    AttributeValue: CreateAttributeValue(x.DataMember.Type, $"{paramReference}.{x.DataMember.Name}")
                )
            )
            .Select(x => (
                    x.DDB,
                    x.AttributeValue,
                    DictionaryAssignment: x.DDB.DataMember.IfStatement($"{paramReference}.{x.DDB.DataMember.Name}",
                        @$"{dictionaryName}.Add(""{x.DDB.AttributeName}"", {x.AttributeValue});")
                )
            )
            .ToArray();


        var dictionary =
            @$"{accessModifier} static Dictionary<string, AttributeValue> {methodName}({parent.Name} {paramReference})
        {{ 
            {InitializeDictionary(dictionaryName, paramReference, properties.Select(x => x.DDB.DataMember))}
            {string.Join(Constants.NewLine + indent, properties.Select(x => x.DictionaryAssignment))}
            return {dictionaryName};
        }}";


        return (dictionary, properties.Select(x => (x.AttributeValue, x.DDB)));
    }

    private static AttributeValueInstance CreateAttributeValue(ITypeSymbol typeSymbol, string accessPattern)
    {
        static string BuildList(ITypeSymbol elementType, string accessPattern)
        {
            var select = $"Select(x => {CreateAttributeValue(elementType, "x")}))";

            return elementType.LambdaExpression() is { } whereBody
                ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
                : $"L = new List<AttributeValue>({accessPattern}.{select}";
        }

        static string? BuildSet(ITypeSymbol elementType, string accessPattern)
        {
            if (elementType.LambdaExpression() is { } lambdaExpression)
                accessPattern = $"{accessPattern}.Where({lambdaExpression})";

            if (elementType.SpecialType is System_String)
                return $"SS = new List<string>({accessPattern})";

            return IsNumeric(elementType) is false
                ? null
                : $"NS = new List<string>({accessPattern}.Select(x => x.ToString()))";
        }

        static bool IsNumeric(ITypeSymbol typeSymbol) => typeSymbol.SpecialType
            is System_Int16 or System_Byte
            or System_Int32 or System_Int64
            or System_SByte or System_UInt16
            or System_UInt32 or System_UInt64
            or System_Decimal or System_Double
            or System_Single;

        static string? SingleGenericTypeOrNull(ITypeSymbol genericType, string accessPattern)
        {
            if (genericType is not INamedTypeSymbol type)
                return null;

            if (type is not {IsGenericType: true, TypeArguments.Length: 1})
                return null;

            var T = type.TypeArguments[0];

            return type switch
            {
                {Name: nameof(Nullable)} => $"{CreateAssignment(T, $"{accessPattern}.Value").Assignment}",
                {Name: "ISet"} => BuildSet(T, accessPattern),
                {Name: nameof(IEnumerable)} => BuildList(T, accessPattern),
                _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => BuildSet(T, accessPattern),
                _ when type.AllInterfaces.Any(x => x is {Name: nameof(IEnumerable)}) => BuildList(T, accessPattern),
                _ => null
            };
        }

        static string? DoubleGenericTypeOrNull(ITypeSymbol genericType, string accessPattern)
        {
            if (genericType is not INamedTypeSymbol type)
                return null;

            if (type is not {IsGenericType: true, TypeArguments.Length: 2})
                return null;

            // ReSharper disable once InconsistentNaming
            var T1 = type.TypeArguments[0];
            // ReSharper disable once InconsistentNaming
            var T2 = type.TypeArguments[1];

            return type switch
            {
                _ when T1.SpecialType is not System_String => null,
                {Name: "ILookup"} =>
                    $"M = {accessPattern}.ToDictionary(x => x.Key, x => new AttributeValue {{ {BuildList(T2, "x")} }})",
                {Name: "IGrouping"} =>
                    $@"M = new Dictionary<string, AttributeValue>{{ {{ {accessPattern}.Key, new AttributeValue {{{BuildList(T2, accessPattern)}}} }} }}",
                {Name: "Dictionary" or "IReadOnlyDictionary" or "IDictionary"} =>
                    $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {CreateAttributeValue(T2, "x.Value")})",
                {Name: "KeyValuePair"} =>
                    $@"M = new Dictionary<string, AttributeValue>() {{ {{{accessPattern}.Key, {CreateAttributeValue(T2, $"{accessPattern}.Value")} }} }}",
                _ => null
            };
        }


        static bool IsTimeRelated(ITypeSymbol symbol)
        {
            return symbol is {SpecialType: System_DateTime} or {Name: nameof(DateTimeOffset) or "DateOnly"};
        }

        static AttributeValueInstance CreateAssignment(ITypeSymbol typeSymbol, string accessPattern)
        {
            var res = typeSymbol switch
            {
                {SpecialType: System_String} => $"S = {accessPattern}",
                {SpecialType: System_Boolean} => $"BOOL = {accessPattern}",
                _ when IsNumeric(typeSymbol) => $"N = {accessPattern}.ToString()",
                _ when IsTimeRelated(typeSymbol) => $@"S = {accessPattern}.ToString(""O"")",
                IArrayTypeSymbol {ElementType: { } elementType} => BuildList(elementType, accessPattern),
                _ when SingleGenericTypeOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                _ when DoubleGenericTypeOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                _ => null
            };

            if (res is null)
            {
                return new AttributeValueInstance(
                    $"M = {Constants.AttributeValueGeneratorMethodName}({accessPattern})",
                    in typeSymbol,
                    AttributeValueInstance.Decision.NeedsExternalInvocation
                );
            }

            return new AttributeValueInstance(in res, in typeSymbol, AttributeValueInstance.Decision.Inlined);
        }

        return CreateAssignment(typeSymbol, accessPattern);
    }

    /// <summary>
    ///   Currently this method is highly dependent in both  AttributeValueGenerator -> this and this -> AttributeValueGenerator
    /// </summary>
    public static bool IsAttributeValueGenerator(this ISymbol type)
    {
        return type
            .GetAttributes()
            .Any(a => a.AttributeClass is
            {
                Name: nameof(AttributeValueGeneratorAttribute),
                ContainingNamespace:
                {
                    Name: nameof(DynamoDBGenerator),
                    ContainingNamespace.IsGlobalNamespace: true
                }
            });
    }
}