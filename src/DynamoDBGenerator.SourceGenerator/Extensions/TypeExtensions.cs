using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    public static Func<ITypeSymbol, string> SuffixedTypeSymbolNameFactory(string? suffix, IEqualityComparer<ISymbol?> comparer)
    {
        var dict = new Dictionary<ITypeSymbol, string>(comparer);

        Func<ITypeSymbol, string> implementation;
        if (Equals(comparer, SymbolEqualityComparer.IncludeNullability))
        {
            implementation = x =>
            {
                if (dict.TryGetValue(x, out var res))
                    return res;

                var displayString = x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                return dict[x] = x switch
                {
                    IArrayTypeSymbol {NullableAnnotation: NullableAnnotation.NotAnnotated} => $"NN_{displayString.ToAlphaNumericMethodName()}_Array{suffix}",
                    IArrayTypeSymbol {NullableAnnotation: NullableAnnotation.None} => $"{displayString.ToAlphaNumericMethodName()}_Array{suffix}",
                    IArrayTypeSymbol {NullableAnnotation: NullableAnnotation.Annotated} => $"N_{displayString.ToAlphaNumericMethodName()}_Array{suffix}",
                    {NullableAnnotation: NullableAnnotation.NotAnnotated} => $"NN_{displayString.ToAlphaNumericMethodName()}{suffix}",
                    {NullableAnnotation: NullableAnnotation.None} => $"{displayString.ToAlphaNumericMethodName()}{suffix}",
                    {NullableAnnotation: NullableAnnotation.Annotated} => $"N_{displayString.ToAlphaNumericMethodName()}{suffix}",
                    _ => throw new NotImplementedException(x.ToDisplayString())
                };

            };
        }
        else
        {
            implementation = x =>
            {
                if (dict.TryGetValue(x, out var res))
                    return res;

                var displayString = x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                return dict[x] = x is IArrayTypeSymbol
                    ? $"{displayString}_Array{suffix}"
                    : $"{displayString.ToAlphaNumericMethodName()}{suffix}";
            };
        }

        return implementation;

    }

    public static Conversion ToConversion(this IEnumerable<string> enumerable)
    {
        return new Conversion(enumerable);
    }
    public static Conversion ToConversion(this IEnumerable<string> enumerable, ITypeSymbol typeSymbol)
    {
        return new Conversion(enumerable, new[] {typeSymbol});
    }

    public static INamedTypeSymbol? TryGetNullableValueType(this ITypeSymbol type)
    {
        return type.IsValueType && type is INamedTypeSymbol {OriginalDefinition.SpecialType: SpecialType.System_Nullable_T} symbol ? symbol : null;
    }

    public static TypeIdentifier GetKnownType(this ITypeSymbol type)
    {
        if (BaseType.CreateInstance(type) is { } baseType)
            return baseType;

        if (SingleGeneric.CreateInstance(type) is { } singleGeneric)
            return singleGeneric;

        if (KeyValueGeneric.CreateInstance(type) is { } keyValueGeneric)
            return keyValueGeneric;

        return new UnknownType(type);
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
    public static IReadOnlyList<DynamoDbDataMember> GetDynamoDbProperties(this ITypeSymbol symbol)
    {
        // A special rule when it comes to Tuples.
        // If we remove this we will get duplicated DataMembers when tuples are being used.
        if (symbol is INamedTypeSymbol {IsTupleType: true} namedTypeSymbol)
        {
            var tupleElements = new List<DynamoDbDataMember>(namedTypeSymbol.TupleElements.Length);
            tupleElements.AddRange(namedTypeSymbol.TupleElements.Select(tupleElement => Create(DataMember.FromField(tupleElement)))
                .Where(dynamoDataMember => dynamoDataMember is not null)
                .Select(dynamoDataMember => dynamoDataMember!.Value));

            return tupleElements;
        }

        var members = symbol.GetMembers();
        var list = new List<DynamoDbDataMember>(members.Length);
        if (symbol.BaseType is {SpecialType: not SpecialType.System_Object})
            list.AddRange(GetDynamoDbProperties(symbol.BaseType));

        list.AddRange(members
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .Where(x => x.CanBeReferencedByName)
            .Select(x => x switch
            {
                IPropertySymbol propertySymbol => Create(DataMember.FromProperty(in propertySymbol)),
                IFieldSymbol fieldSymbol => Create(DataMember.FromField(in fieldSymbol)),
                _ => null
            })
            .Where(x => x is not null)
            .Select(x => x!.Value));

        return list;

        static DynamoDbDataMember? Create(DataMember dataMember)
        {

            var attributes = DynamoDbDataMember.GetDynamoDbAttributes(dataMember.BaseSymbol);
            if (DynamoDbDataMember.IsIgnored(attributes))
                return null;

            return new DynamoDbDataMember(dataMember, attributes);
        }

    }
}