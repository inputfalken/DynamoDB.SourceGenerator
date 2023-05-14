using System.Collections;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public class Generation
{
    private readonly Settings _settings;
    private readonly ITypeSymbol _rootTypeSymbol;
    private const string Indent = "                ";

    public Generation(in Settings settings, in ITypeSymbol typeSymbol)
    {
        _settings = settings;
        _rootTypeSymbol = typeSymbol;
    }

    /// <summary>
    /// Creates an Dictionary with string as key and AttributeValue as value.
    /// </summary>
    public string CreateAttributeValueDictionary()
    {
        var rootMethod = RootAttributeValueConversionMethod();
        var enumerable = ConversionMethods(
                _rootTypeSymbol,
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default)
            )
            .Select(static x => x.Code);

        var sourceGeneration = string.Join(Constants.NewLine, enumerable);

        return @$"{rootMethod}
        private class {_settings.SourceGeneratedClassName}
        {{
            {sourceGeneration}
        }}";
    }

    /// <summary>
    /// Creates an Dictionary with string as key and AttributeUpdate as value.
    /// </summary>
    public string CreateAttributeValueUpdateDictionary()
    {
        throw new NotSupportedException();
    }

    private IEnumerable<Conversion> ConversionMethods(
        ITypeSymbol typeSymbol,
        ISet<ITypeSymbol> typeSymbols
    )
    {
        // We already support the type.
        if (typeSymbols.Add(typeSymbol) is false)
            yield break;

        var dict = StaticDictionaryMethod(typeSymbol);

        yield return dict;

        foreach (var unsupportedType in dict.Conversions)
        {
            if (unsupportedType.AssignedBy is not Assignment.Decision.ExternalMethod)
                continue;

            foreach (var dictionary in ConversionMethods(unsupportedType.Type, typeSymbols))
                yield return dictionary;
        }
    }

    private string RootAttributeValueConversionMethod()
    {
        var config = _settings.ConsumerMethodConfig;

        var accessModifier = _settings.ConsumerMethodConfig.AccessModifier.ToCode();
        var signature = config.MethodParameterization switch
        {
            Settings.ConsumerMethodConfiguration.Parameterization.UnparameterizedInstance =>
                $"{accessModifier} Dictionary<string, AttributeValue> {config.Name}() => {_settings.SourceGeneratedClassName}.{_settings.MPropertyMethodName}(this);",
            Settings.ConsumerMethodConfiguration.Parameterization.ParameterizedStatic =>
                $"{accessModifier} static Dictionary<string, AttributeValue> {config.Name}({_rootTypeSymbol.ToDisplayString()} item) => {_settings.SourceGeneratedClassName}.{_settings.MPropertyMethodName}(item);",
            Settings.ConsumerMethodConfiguration.Parameterization.ParameterizedInstance =>
                $"{accessModifier} Dictionary<string, AttributeValue> {config.Name}({_rootTypeSymbol.ToDisplayString()} item) => {_settings.SourceGeneratedClassName}.{_settings.MPropertyMethodName}(item);",
            _ => throw new NotSupportedException($"Config of '{config.MethodParameterization}'.")
        };


        return $@"/// <summary> 
        ///    Converts <see cref=""{_rootTypeSymbol.ToXmlComment()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        {signature}";
    }

    private Conversion StaticDictionaryMethod(ITypeSymbol type)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";

        IEnumerable<DynamoDbDataMember> dynamoDbDataMembers;

        if (_settings.PredicateConfig is null)
            dynamoDbDataMembers = type.GetDynamoDbProperties();
        else
            dynamoDbDataMembers = type
                .GetDynamoDbProperties()
                .Where(_settings.PredicateConfig.Value.Predicate);

        var properties = dynamoDbDataMembers
            .Where(static x => x.IsIgnored is false)
            .Select(static x => (
                    AccessPattern: $"{paramReference}.{x.DataMember.Name}",
                    DDB: x
                )
            )
            .Select(x => (
                x.AccessPattern,
                x.DDB,
                AttributeValue: CreateAttributeValue(
                    x.DDB.DataMember.Type,
                    x.AccessPattern
                )
            ))
            .Select(static x => (
                    x.DDB,
                    x.AttributeValue,
                    DictionaryAssignment: x.DDB.DataMember.Type.IfStatement(
                        in x.AccessPattern,
                        @$"{dictionaryName}.Add(""{x.DDB.AttributeName}"", {x.AttributeValue});"
                    ),
                    CapacityTernaries: x.DDB.DataMember.Type.TernaryExpression(in x.AccessPattern, "1", "0")
                )
            )
            .ToArray();

        var dictionary =
            @$"        
            /// <summary> 
            ///    Converts <see cref=""{type.ToXmlComment()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
            /// </summary>
            /// <remarks> 
            ///    This method should only be invoked by source generated code.
            /// </remarks>
            public static Dictionary<string, AttributeValue> {_settings.MPropertyMethodName}({type.ToDisplayString()} {paramReference})
            {{ 
                {InitializeDictionary(dictionaryName, properties.Select(static x => x.CapacityTernaries))}
                {string.Join(Constants.NewLine + Indent, properties.Select(static x => x.DictionaryAssignment))}
                return {dictionaryName};
            }}";


        return new Conversion(in dictionary, properties.Select(static x => x.AttributeValue));

        static string InitializeDictionary(string dictionaryName, IEnumerable<string> capacityCalculations)
        {
            var capacityCalculation = string.Join(" + ", capacityCalculations);

            const string capacityReference = "capacity";

            var capacityDeclaration = string.IsNullOrWhiteSpace(capacityCalculation)
                ? $"const int {capacityReference} = 0;"
                : $"var {capacityReference} = {capacityCalculation};";

            var ifCheck = $"if (({capacityReference}) is 0) {{ return {dictionaryName}; }}";

            return @$"{capacityDeclaration}
                var {dictionaryName} = new Dictionary<string, AttributeValue>({capacityReference});
                {ifCheck}";
        }
    }

    private static bool IsTemporal(in ITypeSymbol symbol)
    {
        return symbol is {SpecialType: SpecialType.System_DateTime} or {Name: nameof(DateTimeOffset) or "DateOnly"};
    }

    private Assignment CreateAttributeValue(
        in ITypeSymbol typeSymbol,
        in string accessPattern
    )
    {
        return CreateAssignment(typeSymbol, accessPattern);
    }

    private Assignment BuildList(in ITypeSymbol elementType, in string accessPattern)
    {
        var attributeValue = CreateAttributeValue(elementType, "x");
        var select = $"Select(x => {attributeValue}))";
        var assignment = elementType.LambdaExpression() is { } whereBody
            ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
            : $"L = new List<AttributeValue>({accessPattern}.{select}";

        return new Assignment(in assignment, in elementType, attributeValue.AssignedBy);
    }

    private static Assignment? BuildSet(in ITypeSymbol elementType, in string accessPattern)
    {
        var newAccessPattern = elementType.LambdaExpression() is { } expression
            ? $"{accessPattern}.Where({expression})"
            : accessPattern;

        if (elementType.SpecialType is SpecialType.System_String)
            return new Assignment(
                $"SS = new List<string>({newAccessPattern})",
                in elementType,
                Assignment.Decision.Inline
            );

        return IsNumeric(elementType) is false
            ? null
            : new Assignment(
                $"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))",
                in elementType,
                Assignment.Decision.Inline
            );
    }

    private static bool IsNumeric(in ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType
            is SpecialType.System_Int16 or SpecialType.System_Byte
            or SpecialType.System_Int32 or SpecialType.System_Int64
            or SpecialType.System_SByte or SpecialType.System_UInt16
            or SpecialType.System_UInt32 or SpecialType.System_UInt64
            or SpecialType.System_Decimal or SpecialType.System_Double
            or SpecialType.System_Single;
    }

    private Assignment? SingleGenericTypeOrNull(in ITypeSymbol genericType, in string accessPattern)
    {
        if (genericType is not INamedTypeSymbol type)
            return null;

        if (type is not {IsGenericType: true, TypeArguments.Length: 1})
            return null;

        var T = type.TypeArguments[0];

        return type switch
        {
            {Name: nameof(Nullable)} => CreateAssignment(in T, $"{accessPattern}.Value"),
            {Name: "ISet"} => BuildSet(in T, in accessPattern),
            {Name: nameof(IEnumerable)} => BuildList(in T, in accessPattern),
            _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => BuildSet(in T, in accessPattern),
            _ when type.AllInterfaces.Any(x => x is {Name: nameof(IEnumerable)}) => BuildList(in T,
                in accessPattern),
            _ => null
        };
    }

    private Assignment? DoubleGenericTypeOrNull(in ITypeSymbol genericType, in string accessPattern)
    {
        if (genericType is not INamedTypeSymbol type)
            return null;

        if (type is not {IsGenericType: true, TypeArguments.Length: 2})
            return null;

        // ReSharper disable once InconsistentNaming
        var T1 = type.TypeArguments[0];
        // ReSharper disable once InconsistentNaming
        var T2 = type.TypeArguments[1];

        switch (type)
        {
            case var _ when T1.SpecialType is not SpecialType.System_String:
                return null;
            case {Name: "ILookup"}:
                var lookupValueList = BuildList(in T2, "x");
                return new Assignment(
                    $"M = {accessPattern}.ToDictionary(x => x.Key, x => {lookupValueList})",
                    in T2,
                    lookupValueList.AssignedBy
                );
            case {Name: "IGrouping"}:
                var groupingValueList = BuildList(in T2, accessPattern);
                return new Assignment(
                    $@"M = new Dictionary<string, AttributeValue>{{ {{ {accessPattern}.Key, {groupingValueList}}} }}",
                    in T2,
                    groupingValueList.AssignedBy
                );
            case {Name: "Dictionary" or "IReadOnlyDictionary" or "IDictionary"}:
                var dictionary = CreateAttributeValue(in T2, "x.Value");
                return new Assignment(
                    $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {dictionary})",
                    in T2,
                    dictionary.AssignedBy
                );
            default:
                return null;
        }
    }

    private Assignment CreateAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        var baseTypeConversion = typeSymbol switch
        {
            {SpecialType: SpecialType.System_String} => $"S = {accessPattern}",
            {SpecialType: SpecialType.System_Boolean} => $"BOOL = {accessPattern}",
            {SpecialType: SpecialType.System_Char} => $"S = {accessPattern}.ToString()",
            _ when IsNumeric(in typeSymbol) => $"N = {accessPattern}.ToString()",
            _ when IsTemporal(in typeSymbol) => $@"S = {accessPattern}.ToString(""O"")",
            _ => null
        };

        if (baseTypeConversion is not null)
            return new Assignment(
                in baseTypeConversion,
                in typeSymbol,
                Assignment.Decision.Inline
            );

        Assignment? genericConversion = typeSymbol switch
        {
            _ when SingleGenericTypeOrNull(in typeSymbol, in accessPattern) is { } assignment => assignment,
            _ when DoubleGenericTypeOrNull(in typeSymbol, in accessPattern) is { } assignment => assignment,
            IArrayTypeSymbol {ElementType: { } elementType} => BuildList(in elementType, in accessPattern),
            _ => null
        };

        if (genericConversion is not null)
            return genericConversion.Value;

        return new Assignment(
            $"M = {_settings.MPropertyMethodName}({accessPattern})",
            in typeSymbol,
            Assignment.Decision.ExternalMethod
        );
    }
}