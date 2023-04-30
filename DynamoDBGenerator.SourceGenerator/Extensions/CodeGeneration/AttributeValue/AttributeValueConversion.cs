using System.Collections;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

public static class AttributeValueConversion
{
    public static AttributeValueAssignment CreateAttributeValue(ITypeSymbol typeSymbol, string accessPattern)
    {
        static AttributeValueAssignment BuildList(ITypeSymbol elementType, string accessPattern)
        {
            var attributeValue = CreateAttributeValue(elementType, "x");
            var select = $"Select(x => {attributeValue}))";
            var assignment = elementType.LambdaExpression() is { } whereBody
                ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
                : $"L = new List<AttributeValue>({accessPattern}.{select}";

            return new AttributeValueAssignment(in assignment, in elementType, attributeValue.How);
        }

        static AttributeValueAssignment? BuildSet(ITypeSymbol elementType, string accessPattern)
        {
            if (elementType.LambdaExpression() is { } lambdaExpression)
                accessPattern = $"{accessPattern}.Where({lambdaExpression})";

            if (elementType.SpecialType is SpecialType.System_String)
                return new AttributeValueAssignment(
                    $"SS = new List<string>({accessPattern})",
                    in elementType,
                    AttributeValueAssignment.Decision.Inlined
                );

            return IsNumeric(elementType) is false
                ? null
                : new AttributeValueAssignment(
                    $"NS = new List<string>({accessPattern}.Select(x => x.ToString()))",
                    in elementType,
                    AttributeValueAssignment.Decision.Inlined
                );
        }

        static bool IsNumeric(ITypeSymbol typeSymbol) => typeSymbol.SpecialType
            is SpecialType.System_Int16 or SpecialType.System_Byte
            or SpecialType.System_Int32 or SpecialType.System_Int64
            or SpecialType.System_SByte or SpecialType.System_UInt16
            or SpecialType.System_UInt32 or SpecialType.System_UInt64
            or SpecialType.System_Decimal or SpecialType.System_Double
            or SpecialType.System_Single;

        static AttributeValueAssignment? SingleGenericTypeOrNull(ITypeSymbol genericType, string accessPattern)
        {
            if (genericType is not INamedTypeSymbol type)
                return null;

            if (type is not {IsGenericType: true, TypeArguments.Length: 1})
                return null;

            var T = type.TypeArguments[0];

            return type switch
            {
                {Name: nameof(Nullable)} => CreateAssignment(T, $"{accessPattern}.Value"),
                {Name: "ISet"} => BuildSet(T, accessPattern),
                {Name: nameof(IEnumerable)} => BuildList(T, accessPattern),
                _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => BuildSet(T, accessPattern),
                _ when type.AllInterfaces.Any(x => x is {Name: nameof(IEnumerable)}) => BuildList(T, accessPattern),
                _ => null
            };
        }

        static AttributeValueAssignment? DoubleGenericTypeOrNull(ITypeSymbol genericType, string accessPattern)
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
                    var lookupValueList = BuildList(T2, "x");
                    return new AttributeValueAssignment(
                        $"M = {accessPattern}.ToDictionary(x => x.Key, x => {lookupValueList})",
                        in T2,
                        lookupValueList.How
                    );
                case {Name: "IGrouping"}:
                    var groupingValueList = BuildList(T2, accessPattern);
                    return new AttributeValueAssignment(
                        $@"M = new Dictionary<string, AttributeValue>{{ {{ {accessPattern}.Key, {groupingValueList}}} }}",
                        in T2,
                        groupingValueList.How
                    );
                case {Name: "Dictionary" or "IReadOnlyDictionary" or "IDictionary"}:
                    var dictionary = CreateAttributeValue(T2, "x.Value");
                    return new AttributeValueAssignment(
                        $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {dictionary})",
                        in T2,
                        dictionary.How
                    );
                case {Name: "KeyValuePair"}:
                    var keyValuePair = CreateAttributeValue(T2, $"{accessPattern}.Value");
                    return new AttributeValueAssignment(
                        $@"M = new Dictionary<string, AttributeValue>() {{ {{{accessPattern}.Key, {keyValuePair} }} }}",
                        in T2,
                        keyValuePair.How
                    );
                default:
                    return null;
            }
        }


        static bool IsTimeRelated(ITypeSymbol symbol)
        {
            return symbol is {SpecialType: SpecialType.System_DateTime} or {Name: nameof(DateTimeOffset) or "DateOnly"};
        }

        static AttributeValueAssignment CreateAssignment(ITypeSymbol typeSymbol, string accessPattern)
        {
            var baseTypeConversion = typeSymbol switch
            {
                {SpecialType: SpecialType.System_String} => $"S = {accessPattern}",
                {SpecialType: SpecialType.System_Boolean} => $"BOOL = {accessPattern}",
                _ when IsNumeric(typeSymbol) => $"N = {accessPattern}.ToString()",
                _ when IsTimeRelated(typeSymbol) => $@"S = {accessPattern}.ToString(""O"")",
                _ => null
            };

            if (baseTypeConversion is not null)
                return new AttributeValueAssignment(
                    in baseTypeConversion,
                    in typeSymbol,
                    AttributeValueAssignment.Decision.Inlined
                );

            AttributeValueAssignment? genericConversion = typeSymbol switch
            {
                _ when SingleGenericTypeOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                _ when DoubleGenericTypeOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                IArrayTypeSymbol {ElementType: { } elementType} => BuildList(elementType, accessPattern),
                _ => null
            };


            if (genericConversion is not null)
                return genericConversion.Value;

            return new AttributeValueAssignment(
                $"M = {Constants.AttributeValueGeneratorMethodName}({accessPattern})",
                in typeSymbol,
                AttributeValueAssignment.Decision.NeedsExternalInvocation
            );
        }

        return CreateAssignment(typeSymbol, accessPattern);
    }
}