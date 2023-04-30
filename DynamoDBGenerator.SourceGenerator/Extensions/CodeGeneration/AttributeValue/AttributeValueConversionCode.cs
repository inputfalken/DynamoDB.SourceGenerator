using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

public static class AttributeValueConversionCode
{
    public static string CreateAttributeConversionCode(this ITypeSymbol type)
    {
        var conversionMethods = ConversionMethods(type, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default))
            .Select(x => x.Code)
            .Prepend(RootAttributeValueConversionMethod(Constants.AttributeValueGeneratorMethodName));

        return string.Join(Constants.NewLine, conversionMethods);

        IEnumerable<MapToAttributeValueMethod> ConversionMethods(
            ITypeSymbol typeSymbol,
            ISet<ITypeSymbol> typeSymbols
        )
        {
            // We already support the type.
            if (typeSymbols.Add(typeSymbol) is false)
                yield break;

            var dict = typeSymbol
                .StaticAttributeValueConversionMethod(Constants.AttributeValueGeneratorMethodName);

            yield return dict;

            var unsupportedTypes = dict.Conversions
                .Where(x => x.Towards.How == AttributeValueAssignment.Decision.NeedsExternalInvocation)
                .Select(x => x.Towards.Type);

            foreach (var unsupportedType in unsupportedTypes)
            foreach (var dictionary in ConversionMethods(unsupportedType, typeSymbols))
                yield return dictionary;
        }
    }

    private static string RootAttributeValueConversionMethod(string methodName)
    {
        return $@"[MethodImpl(MethodImplOptions.AggressiveInlining)]
public Dictionary<string, AttributeValue> {methodName}() => {methodName}(this);";
    }


    private static MapToAttributeValueMethod StaticAttributeValueConversionMethod(
        this INamespaceOrTypeSymbol parent,
        string methodName,
        string accessModifier = Constants.AccessModifiers.Private
    )
    {
        var paramReference = parent.Name
            .FirstCharToLower();

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
        var properties = parent
            .GetDynamoDbProperties()
            .Where(x => x.IsIgnored is false)
            .Select(x => (
                    DDB: x,
                    AttributeValue: AttributeValueConversion.CreateAttributeValue(x.DataMember.Type,
                        $"{paramReference}.{x.DataMember.Name}")
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


        const string indent = "            ";
        var dictionary =
            @$"{accessModifier} static Dictionary<string, AttributeValue> {methodName}({parent.ToDisplayString()} {paramReference})
        {{ 
            {InitializeDictionary(dictionaryName, paramReference, properties.Select(x => x.DDB.DataMember))}
            {string.Join(Constants.NewLine + indent, properties.Select(x => x.DictionaryAssignment))}
            return {dictionaryName};
        }}";


        return new MapToAttributeValueMethod(
            in dictionary,
            properties.Select(
                x => new Conversion<DynamoDbDataMember, AttributeValueAssignment>(in x.DDB, in x.AttributeValue)
            )
        );
    }
}