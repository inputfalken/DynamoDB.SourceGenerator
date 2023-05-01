using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

public static class AttributeValueConversionCode
{
    /// <summary>
    ///     Generated attribute value conversion.
    /// </summary>
    /// <param name="type">
    ///     The root type to create attribute value conversions from.
    /// </param>
    /// <param name="settings">
    ///     Instructions for how the internal source generator should  perform its generations.
    /// </param>
    /// <param name="consumerMethodName">
    ///     The instance method name that the consumer of the source generated code will be invoking.
    /// </param>
    /// <returns></returns>
    public static string CreateAttributeConversionCode(
        this ITypeSymbol type,
        in AttributeValueConversionSettings settings,
        string consumerMethodName
    )
    {
        var conversionMethods = ConversionMethods(
                type,
                settings,
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default)
            )
            .Select(x => x.Code)
            .Prepend(RootAttributeValueConversionMethod(in type, in settings, in consumerMethodName));

        return string.Join(Constants.NewLine, conversionMethods);

        static IEnumerable<MapToAttributeValueMethod> ConversionMethods(
            ITypeSymbol typeSymbol,
            AttributeValueConversionSettings settings,
            ISet<ITypeSymbol> typeSymbols
        )
        {
            // We already support the type.
            if (typeSymbols.Add(typeSymbol) is false)
                yield break;

            var dict = typeSymbol.StaticDictionaryMethod(settings);

            yield return dict;

            var unsupportedTypes = dict.Conversions
                .Where(x => x.Towards.AssignedBy is AttributeValueAssignment.Decision.ExternalMethod)
                .Select(x => x.Towards.Type);

            foreach (var unsupportedType in unsupportedTypes)
            foreach (var dictionary in ConversionMethods(unsupportedType, settings, typeSymbols))
                yield return dictionary;
        }
    }

    private static string RootAttributeValueConversionMethod(
        in ITypeSymbol type,
        in AttributeValueConversionSettings settings,
        in string consumerMethodName
    )
    {
        return $@"
        /// <summary> 
        ///    Converts <see cref=""{type.ToDisplayString()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, AttributeValue> {consumerMethodName}() => {settings.MPropertyMethodName}(this);";
    }

    private static MapToAttributeValueMethod StaticDictionaryMethod(this INamespaceOrTypeSymbol type,
        AttributeValueConversionSettings settings)
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
            .Zip(
                Enumerable.Repeat(new AttributeValueConversion(settings), int.MaxValue),
                (x, y) => (x.AccessPattern, x.DDB, AttributeValueConverter: y)
            )
            .Select(x => (
                x.AccessPattern,
                x.DDB,
                AttributeValue: x.AttributeValueConverter.CreateAttributeValue(
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
            @$"        
        /// <summary> 
        ///    Converts <see cref=""{type.ToDisplayString()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
        /// </summary>
        /// <remarks> 
        ///    This method should only be invoked by source generated code.
        /// </remarks>
        private static Dictionary<string, AttributeValue> {settings.MPropertyMethodName}({type.ToDisplayString()} {paramReference})
        {{ 
            {InitializeDictionary(dictionaryName, properties.Select(x => x.CapacityTernaries))}
            {string.Join(Constants.NewLine + indent, properties.Select(x => x.DictionaryAssignment))}
            return {dictionaryName};
        }}";


        return new MapToAttributeValueMethod(
            in dictionary,
            settings.MPropertyMethodName,
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