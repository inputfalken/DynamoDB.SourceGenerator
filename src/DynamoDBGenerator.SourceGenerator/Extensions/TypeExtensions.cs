using System.Collections.Concurrent;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeExtensions
{
    public static Func<ITypeSymbol, T> CacheFactory<T>(IEqualityComparer<ISymbol> comparer, Func<ITypeSymbol, T> selector)
    {
        var cache = new Dictionary<ITypeSymbol, T>(comparer);

        return x => cache.TryGetValue(x, out var value) ? value : cache[x] = selector(x);
    }
    
    public static Func<ITypeSymbol, string> SuffixedTypeSymbolNameFactory(string? suffix, IEqualityComparer<ISymbol?> comparer, bool useNullableAnnotationNaming)
    {
        return x => Execution(
            new Dictionary<ITypeSymbol, string>(comparer),
            x,
            false,
            suffix,
            useNullableAnnotationNaming
        );

        static string Execution(
            IDictionary<ITypeSymbol, string> cache,
            ITypeSymbol typeSymbol,
            bool isRecursive,
            string? suffix,
            bool useNullableAnnotationNaming
        )
        {
            if (cache.TryGetValue(typeSymbol, out var methodName))
                return methodName;

            var displayString = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var str = useNullableAnnotationNaming
                ? (typeSymbol.NullableAnnotation, typeDisplay: displayString) switch
                {
                    (NullableAnnotation.NotAnnotated, _) => $"NN_{displayString.ToAlphaNumericMethodName()}{suffix}",
                    (NullableAnnotation.None, _) => $"{displayString.ToAlphaNumericMethodName()}{suffix}",
                    (NullableAnnotation.Annotated, _) => $"N_{displayString.ToAlphaNumericMethodName()}{suffix}",
                    _ => throw new NotImplementedException(typeSymbol.ToDisplayString())
                }
                : $"{displayString.ToAlphaNumericMethodName()}{suffix}";

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                // We do not need to populate the dictionary if the execution originates from recursion.
                if (isRecursive is false)
                    cache[typeSymbol] = str;

                return str;
            }

            var result = string.Join(
                "_",
                namedTypeSymbol.TypeArguments.Select(x => Execution(cache, x, true, suffix, useNullableAnnotationNaming)).Prepend(str)
            );

            // We do not need to populate the dictionary if the execution originates from recursion.
            if (isRecursive is false)
                cache[typeSymbol] = result;

            return result;
        }
    }
    public static Assignment ToInlineAssignment(this ITypeSymbol typeSymbol, string value, KnownType knownType)
    {
        return new Assignment(in value, typeSymbol, knownType);
    }

    public static Assignment ToExternalDependencyAssignment(this ITypeSymbol typeSymbol, string value)
    {
        return new Assignment(in value, in typeSymbol, null);
    }

    public static INamedTypeSymbol? TryGetNullableValueType(this ITypeSymbol type)
    {
        return type.IsValueType && type is INamedTypeSymbol {OriginalDefinition.SpecialType: SpecialType.System_Nullable_T} symbol ? symbol : null;
    }

    public static KnownType? GetKnownType(this ITypeSymbol type)
    {
        if (BaseType.CreateInstance(type) is { } baseType)
            return baseType;

        if (SingleGeneric.CreateInstance(type) is { } singleGeneric)
            return singleGeneric;

        if (KeyValueGeneric.CreateInstance(type) is { } keyValueGeneric)
            return keyValueGeneric;

        return null;
    }
    public static bool IsNumeric(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType
            is SpecialType.System_Int16
            or SpecialType.System_Byte
            or SpecialType.System_Int32
            or SpecialType.System_Int64
            or SpecialType.System_SByte
            or SpecialType.System_UInt16
            or SpecialType.System_UInt32
            or SpecialType.System_UInt64
            or SpecialType.System_Decimal
            or SpecialType.System_Double
            or SpecialType.System_Single;
    }
}