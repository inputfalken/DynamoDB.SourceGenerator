using System.Text;
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

            var dict = typeSymbol.StaticDictionaryMethod(Constants.AttributeValueGeneratorMethodName);

            yield return dict;

            var unsupportedTypes = dict.Conversions
                .Where(x => x.Towards.AssignedBy is AttributeValueAssignment.Decision.ExternalMethod)
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


    private static MapToAttributeValueMethod StaticDictionaryMethod(this INamespaceOrTypeSymbol type, string name)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = type
            .GetDynamoDbProperties()
            .Where(x => x.IsIgnored is false)
            .Select(x => (
                    AccessPattern: $"{paramReference}.{x.DataMember.Name}",
                    DDB: x
                )
            )
            .Select(x => (
                x.AccessPattern,
                x.DDB,
                AttributeValue: AttributeValueConversion.CreateAttributeValue(
                    x.DDB.DataMember.Type,
                    x.AccessPattern
                )
            ))
            .Select(x => (
                    x.DDB,
                    x.AttributeValue,
                    DictionaryAssignment: x.DDB.DataMember.IfStatement(
                        x.AccessPattern,
                        @$"{dictionaryName}.Add(""{x.DDB.AttributeName}"", {x.AttributeValue});"
                    ),
                    CapacityTernaries: x.DDB.DataMember.Type.TernaryExpression(x.AccessPattern, "1", "0")
                )
            )
            .ToArray();

        const string indent = "            ";
        var dictionary =
            @$"        private static Dictionary<string, AttributeValue> {name}({type.ToDisplayString()} {paramReference})
        {{ 
            {InitializeDictionary(dictionaryName, properties.Select(x => x.CapacityTernaries))}
            {string.Join(Constants.NewLine + indent, properties.Select(x => x.DictionaryAssignment))}
            return {dictionaryName};
        }}";


        return new MapToAttributeValueMethod(
            in dictionary,
            properties.Select(
                x => new Conversion<DynamoDbDataMember, AttributeValueAssignment>(in x.DDB, in x.AttributeValue)
            )
        );

        static string InitializeDictionary(string dictionaryName, IEnumerable<string> capacityCalculations)
        {
            var capacityCalculation = string.Join(" + ", capacityCalculations);

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
    }
}