using System.Collections.Concurrent;
using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public abstract record TypeIdentifier
{
    private static readonly ConcurrentDictionary<ITypeSymbol, (string, string)> RepresentationDictionary =
        new(SymbolEqualityComparer.IncludeNullability);

    internal static readonly IEqualityComparer<TypeIdentifier> Default =
        new TypeSymbolDelegator(SymbolEqualityComparer.Default);

    internal static readonly IEqualityComparer<TypeIdentifier> Nullable =
        new TypeSymbolDelegator(SymbolEqualityComparer.IncludeNullability);

    protected TypeIdentifier(ITypeSymbol typeSymbol)
    {
        IsNullable = typeSymbol switch
        {
            {
                IsReferenceType: true, NullableAnnotation: NullableAnnotation.None or NullableAnnotation.Annotated
            } => true,
            { IsReferenceType: true, NullableAnnotation: NullableAnnotation.NotAnnotated } => false,
            { IsValueType: true, OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } => true,
            { IsValueType: true } => false,
            _ => throw new ArgumentOutOfRangeException(
                $"Could not determine nullablity of type '{typeSymbol.ToDisplayString()}'.")
        };

        TypeSymbol = typeSymbol;
        var (annotated, original) = Representation(typeSymbol);
        UnannotatedString = original;
        AnnotatedString = annotated;
        IsNumeric = IsNumericMethod(typeSymbol);
    }

    public bool IsNullable { get; }
    public bool IsNumeric { get; }
    public string AnnotatedString { get; }
    public string UnannotatedString { get; }

    public ITypeSymbol TypeSymbol { get; }

    private static (string annotated, string original) Representation(ITypeSymbol typeSymbol)
    {
        return RepresentationDictionary.GetOrAdd(typeSymbol, x =>
        {
            var displayString = x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return RepresentationDictionary[typeSymbol] = (ToString(typeSymbol, displayString), displayString);
        });

        static string ToString(ITypeSymbol x, string displayString)
        {
            if (x is IArrayTypeSymbol arrayTypeSymbol)
            {
                var result = ToString(arrayTypeSymbol.ElementType,
                    arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                return x.NullableAnnotation switch
                {
                    NullableAnnotation.Annotated or NullableAnnotation.None => $"{result}[]?",
                    NullableAnnotation.NotAnnotated => $"{result}[]",
                    _ => throw new ArgumentException(ExceptionMessage(x))
                };
            }

            if (x is not INamedTypeSymbol namedTypeSymbol || namedTypeSymbol.TypeArguments.Length is 0)
                return x.NullableAnnotation switch
                {
                    // Having `Annotated` and `None` produce append '?' is fine as long as `SuffixedTypeSymbolNameFactory` is giving them different names. Otherwise we could create broken signatures due to duplication.
                    NullableAnnotation.Annotated or NullableAnnotation.None => $"{displayString}?",
                    NullableAnnotation.NotAnnotated => displayString,
                    _ => throw new ArgumentException(ExceptionMessage(x))
                };

            if (namedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
                return displayString;

            if (namedTypeSymbol.IsTupleType)
            {
                var tupleElements = namedTypeSymbol.TupleElements
                    .Select(y =>
                        $"{ToString(y.Type, $"{y.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}")} {y.Name}");
                return $"({string.Join(", ", tupleElements)})";
            }

            var index = displayString.AsSpan().IndexOf('<');
            if (index == -1)
                return displayString;

            var typeWithoutGenericParameters = displayString.Substring(0, index);
            var typeParameters = string.Join(", ",
                namedTypeSymbol.TypeArguments.Select(y =>
                    ToString(y, y.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))));
            return namedTypeSymbol.NullableAnnotation switch
            {
                // Having `Annotated` and `None` produce append '?' is fine as long as `SuffixedTypeSymbolNameFactory` is giving them different names. Otherwise we could create broken signatures due to duplication.
                NullableAnnotation.Annotated or NullableAnnotation.None =>
                    $"{typeWithoutGenericParameters}<{typeParameters}>?",
                NullableAnnotation.NotAnnotated => $"{typeWithoutGenericParameters}<{typeParameters}>",
                _ => throw new ArgumentException(ExceptionMessage(namedTypeSymbol))
            };

            static string ExceptionMessage(ISymbol typeSymbol)
            {
                return $"Could nullable annotation on type: {typeSymbol.ToDisplayString()}";
            }
        }
    }

    private static bool IsNumericMethod(ITypeSymbol typeSymbol)
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

    public string ReturnNullOrThrow(string dataMember)
    {
        return IsNullable
            ? "return null;"
            : $"throw {Constants.DynamoDBGenerator.ExceptionHelper.NullExceptionMethod}({dataMember});";
    }

    private class TypeSymbolDelegator : IEqualityComparer<TypeIdentifier>
    {
        private readonly IEqualityComparer<ITypeSymbol> _comparer;

        public TypeSymbolDelegator(IEqualityComparer<ITypeSymbol> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(TypeIdentifier? x, TypeIdentifier? y)
        {
            return ReferenceEquals(x, y) || x switch
            {
                null => false,
                _ => y is not null && _comparer.Equals(x.TypeSymbol, y.TypeSymbol)
            };
        }

        public int GetHashCode(TypeIdentifier obj)
        {
            return _comparer.GetHashCode(obj.TypeSymbol);
        }
    }
}

public sealed record UnknownType(ITypeSymbol TypeSymbol) : TypeIdentifier(TypeSymbol);

public sealed record KeyValueGeneric : TypeIdentifier
{
    public enum SupportedType
    {
        LookUp = 1,
        Dictionary = 2
    }

    private KeyValueGeneric(in ITypeSymbol typeSymbol, in ITypeSymbol tKey, in ITypeSymbol tValue,
        in SupportedType supportedType) : base(typeSymbol)
    {
        Type = supportedType;
        TKey = tKey;
        TValue = tValue.TypeIdentifier();
    }

    public SupportedType Type { get; }

    // ReSharper disable once InconsistentNaming
    public ITypeSymbol TKey { get; }

    // ReSharper disable once InconsistentNaming
    public TypeIdentifier TValue { get; }

    public static KeyValueGeneric? CreateInstance(in ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol type)
            return null;

        if (type is not { IsGenericType: true, TypeArguments.Length: 2 })
            return null;

        SupportedType? supported = type switch
        {
            { Name: "ILookup" } => SupportedType.LookUp,
            { Name: "Dictionary" or "IReadOnlyDictionary" or "IDictionary" } => SupportedType.Dictionary,
            _ => null
        };
        return supported is null
            ? null
            : new KeyValueGeneric(typeSymbol, type.TypeArguments[0], type.TypeArguments[1], supported.Value);
    }
}

public sealed record SingleGeneric : TypeIdentifier
{
    public enum SupportedType
    {
        Nullable = 1,
        Set = 2,
        Array = 3,
        ICollection = 4,
        IReadOnlyCollection = 5,
        IEnumerable = 6,
        List = 7
    }

    private SingleGeneric(ITypeSymbol type, ITypeSymbol innerType, in SupportedType supportedType) : base(type)

    {
        Type = supportedType;
        T = innerType.TypeIdentifier();
    }

    public SupportedType Type { get; }
    public TypeIdentifier T { get; }

    public static SingleGeneric? CreateInstance(in ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            return new SingleGeneric(typeSymbol, arrayTypeSymbol.ElementType, SupportedType.Array);

        if (typeSymbol is not INamedTypeSymbol type)
            return null;

        if (type is not { IsGenericType: true, TypeArguments.Length: 1 })
            return null;

        SupportedType? supported = type switch
        {
            _ when type.TryGetNullableValueType() is not null => SupportedType.Nullable,
            _ when type.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.List<T>" => SupportedType
                .List,
            { Name: "ISet" } => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is { Name: "ISet" }) => SupportedType.Set,
            { Name: "IReadOnlySet" } => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is { Name: "IReadOnlySet" }) => SupportedType.Set,
            { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T } => SupportedType
                .ICollection,
            _ when type.AllInterfaces.Any(x => x is
                    { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T }) =>
                SupportedType.ICollection,
            { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T } =>
                SupportedType.IReadOnlyCollection,
            _ when type.AllInterfaces.Any(x => x is
                    { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T }) =>
                SupportedType.IReadOnlyCollection,
            { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } => SupportedType
                .IEnumerable,
            _ => null
        };

        return supported is null ? null : new SingleGeneric(type, type.TypeArguments[0], supported.Value);
    }
}