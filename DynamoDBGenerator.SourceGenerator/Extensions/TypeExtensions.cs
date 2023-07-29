using System.Text.RegularExpressions;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeExtensions
{

    public static Func<ITypeSymbol, string> NameCache(SymbolDisplayFormat symbolDisplayFormat ,IEqualityComparer<ISymbol> comparer)
    {
        var dictionary = new Dictionary<ISymbol, string>(comparer);

        return x =>
        {
            if (dictionary.TryGetValue(x, out var name))
                return name;

            name = x.ToDisplayString(symbolDisplayFormat);
            
            dictionary.Add(x, name);
            return name;
        };
    }
    public static Func<ITypeSymbol, string> TypeSymbolStringCache(string suffix, IEqualityComparer<ISymbol> comparer)
    {
        return x => Execution(
            new Dictionary<ITypeSymbol, string>(comparer),
            x,
            false,
            suffix
        );

        static string Execution(
            IDictionary<ITypeSymbol, string> cache,
            ITypeSymbol typeSymbol,
            bool isRecursive,
            string suffix
        )
        {
            if (cache.TryGetValue(typeSymbol, out var methodName))
                return methodName;

            var displayString = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var str = (typeSymbol.NullableAnnotation, typeDisplay: displayString) switch
            {
                (_, {Length: > Constants.MaxMethodNameLenght}) => throw new NotSupportedException(
                    $"Could not generate a method name that's within the supported method lenght {Constants.MaxMethodNameLenght} for type '{displayString}'."),
                (NullableAnnotation.NotAnnotated, _) => $"NN_{displayString.ToAlphaNumericMethodName()}{suffix}",
                (NullableAnnotation.None, _) => $"{displayString.ToAlphaNumericMethodName()}{suffix}",
                (NullableAnnotation.Annotated, _) => $"N_{displayString.ToAlphaNumericMethodName()}{suffix}",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                // We do not need to populate the dictionary if the execution originates from recursion.
                if (isRecursive is false)
                    cache[typeSymbol] = str;

                return str;
            }

            var result = string.Join(
                "_",
                namedTypeSymbol.TypeArguments.Select(x => Execution(cache, x, true, suffix)).Prepend(str)
            );

            // We do not need to populate the dictionary if the execution originates from recursion.
            if (isRecursive is false)
                cache[typeSymbol] = result;

            return result;
        }
    }
    public static Assignment ToInlineAssignment(this ITypeSymbol typeSymbol, string value)
    {
        return new Assignment(in value, in typeSymbol, false);
    }

    public static Assignment ToExternalDependencyAssignment(this ITypeSymbol typeSymbol, string value)
    {
        return new Assignment(in value, in typeSymbol, true);
    }

    public static string ToXmlComment(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return typeSymbol.ToDisplayString();

        var typeParameters = string.Join(",", namedTypeSymbol.TypeParameters.Select(x => x.Name));

        return Regex.Replace(namedTypeSymbol.ToDisplayString(), "<.+>", $"{{{typeParameters}}}");
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