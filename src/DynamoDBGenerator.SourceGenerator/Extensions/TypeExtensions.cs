using System.Collections.Concurrent;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeExtensions
{
    private static readonly Dictionary<ITypeSymbol, TypeIdentifier> TypeIdentifierDictionary =
        new(SymbolEqualityComparer.IncludeNullability);

    private static readonly SymbolDisplayFormat DisplayFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    public static IEnumerable<string> NamespaceDeclaration(this INamedTypeSymbol type, IEnumerable<string> content)
    {
        return type.ContainingNamespace.IsGlobalNamespace
            ? content
            : $"namespace {type.ContainingNamespace.ToDisplayString()}".CreateScope(content);
    }

    public static string TypeDeclaration(this INamedTypeSymbol type)
    {
        if (type.DeclaredAccessibility is not Accessibility.Public)
            throw new NotImplementedException(
                $"Generate accessibility of '{type.DeclaredAccessibility}' on '{type.ToDisplayParts()}' only '{type.DeclaredAccessibility == Accessibility.Public}' is supported."
            );

        return type switch
        {
            { IsRecord: true, TypeKind: TypeKind.Class, IsSealed: true } =>
                $"public {(type.IsStatic ? "static " : null)}sealed partial record {type.Name}",
            { IsRecord: true, TypeKind: TypeKind.Class, IsSealed: false } =>
                $"public {(type.IsStatic ? "static " : null)}partial record {type.Name}",
            { IsRecord: false, TypeKind: TypeKind.Class, IsSealed: true } =>
                $"public {(type.IsStatic ? "static " : null)}sealed partial class {type.Name}",
            { IsRecord: false, TypeKind: TypeKind.Class, IsSealed: false } =>
                $"public {(type.IsStatic ? "static " : null)}partial class {type.Name}",
            { IsRecord: true, TypeKind: TypeKind.Struct, IsReadOnly: true } =>
                $"public {(type.IsStatic ? "static " : null)}readonly partial record struct {type.Name}",
            { IsRecord: false, TypeKind: TypeKind.Struct, IsReadOnly: true } =>
                $"public {(type.IsStatic ? "static " : null)}readonly partial struct {type.Name}",
            { IsRecord: false, TypeKind: TypeKind.Struct, IsReadOnly: false } =>
                $"public {(type.IsStatic ? "static " : null)}partial struct {type.Name}",
            _ => throw new NotImplementedException("Could not determine whether the type is a struct, class or record.")
        };
    }

    public static Func<ITypeSymbol, T> CacheFactory<T>(IEqualityComparer<ISymbol> comparer,
        Func<ITypeSymbol, T> selector)
    {
        var cache = new Dictionary<ITypeSymbol, T>(comparer);

        return x => cache.TryGetValue(x, out var value) ? value : cache[x] = selector(x);
    }

    public static TypeIdentifier TypeIdentifier(this ITypeSymbol type)
    {
        if (TypeIdentifierDictionary.TryGetValue(type, out var typeIdentifier))
            return typeIdentifier;

        return TypeIdentifierDictionary[type] = Create(type);

        static TypeIdentifier Create(ITypeSymbol typeSymbol)
        {
            if (SingleGeneric.CreateInstance(typeSymbol) is { } singleGeneric)
                return singleGeneric;

            if (KeyValueGeneric.CreateInstance(typeSymbol) is { } keyValueGeneric)
                return keyValueGeneric;

            return new UnknownType(typeSymbol);
        }
    }

    public static Func<ITypeSymbol, ITypeSymbol, string> SuffixedFullyQualifiedTypeName(string suffix,
        IEqualityComparer<ISymbol?> comparer)
    {
        IEqualityComparer<(ITypeSymbol, ITypeSymbol)> tupleComparer = new TupleComparer(comparer);
        var dict = new ConcurrentDictionary<(ITypeSymbol, ITypeSymbol), string>(tupleComparer);
        if (Equals(comparer, SymbolEqualityComparer.Default))
            return (x, y) => dict.GetOrAdd(
                (x, y),
                tuple =>
                {
                    var (p, c) = tuple;
                    return c is IArrayTypeSymbol
                        ? $"{p.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{c.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}_{c.BaseType!.ToDisplayString()}{suffix}"
                        : $"{p.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{c.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).ToAlphaNumericMethodName()}{suffix}";
                });

        throw new NotSupportedException(
            $"Method {nameof(SuffixedFullyQualifiedTypeName)} does not implement equality comparer '{comparer.GetType().Name}"
        );
    }

    public static Func<ITypeSymbol, string> SuffixedTypeSymbolNameFactory(string? suffix,
        IEqualityComparer<ISymbol?> comparer)
    {
        var dict = new ConcurrentDictionary<ITypeSymbol, string>(comparer);

        if (Equals(comparer, SymbolEqualityComparer.IncludeNullability))
            if (suffix is null)
                return x => dict.GetOrAdd(x, NullableAnnotation);
            else
                return x => dict.GetOrAdd(x, y => $"{NullableAnnotation(y)}{suffix}");

        if (suffix is null)
            return x => dict.GetOrAdd(x,
                y => y is IArrayTypeSymbol
                    ? $"{y.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}_{y.BaseType!.ToDisplayString()}"
                    : y.ToDisplayString(
                        DisplayFormat).ToAlphaNumericMethodName());

        return x => dict.GetOrAdd(x,
            y => y is IArrayTypeSymbol
                ? $"{y.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}_{y.BaseType!.ToDisplayString()}{suffix}"
                : $"{y.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).ToAlphaNumericMethodName()}{suffix}"
        );

        static string NullableAnnotation(ITypeSymbol x)
        {
            return x switch
            {
                // Could cause a NullReference exception but very unlikely since all IArrayTypeSymbol should inherit from Array.
                IArrayTypeSymbol
                    {
                        NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated
                    } array => $"NN_{array.BaseType!.Name}_{NullableAnnotation(array.ElementType)}",
                IArrayTypeSymbol { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.None } array =>
                    $"{array.BaseType!.Name}_{NullableAnnotation(array.ElementType)}",
                IArrayTypeSymbol { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.Annotated } array
                    => $"N_{array.BaseType!.Name}_{NullableAnnotation(array.ElementType)}",
                INamedTypeSymbol
                    {
                        OriginalDefinition.SpecialType: not SpecialType.System_Nullable_T,
                        TypeArguments.Length : > 0, IsTupleType: false
                    } namedTypeSymbol
                    when string.Join("_", namedTypeSymbol.TypeArguments.Select(NullableAnnotation)) is var a
                    => namedTypeSymbol switch
                    {
                        { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated } =>
                            $"NN_{namedTypeSymbol.Name}_{a}",
                        { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.None } =>
                            $"{namedTypeSymbol.Name}_{a}",
                        { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.Annotated } =>
                            $"N_{namedTypeSymbol.Name}_{a}",
                        _ => throw new NotImplementedException(ExceptionMessage(namedTypeSymbol))
                    },
                INamedTypeSymbol { IsTupleType: true } single when string.Join("_",
                        single.TupleElements.Select(y => $"{y.Name}_{NullableAnnotation(y.Type)}")) is var tuple =>
                    single switch
                    {
                        { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.None } => tuple,
                        { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.Annotated } => $"N_{tuple}",
                        {
                                NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated
                            } => $"NN_{tuple}",
                        _ => throw new NotImplementedException(ExceptionMessage(single))
                    },
                { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated } =>
                    $"NN_{x.ToDisplayString(DisplayFormat).ToAlphaNumericMethodName()}",
                { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.None } =>
                    $"{x.ToDisplayString(DisplayFormat).ToAlphaNumericMethodName()}",
                { NullableAnnotation: Microsoft.CodeAnalysis.NullableAnnotation.Annotated } =>
                    $"N_{x.ToDisplayString(DisplayFormat).ToAlphaNumericMethodName()}",
                _ => throw new NotImplementedException(ExceptionMessage(x))
            };

            static string ExceptionMessage(ITypeSymbol typeSymbol)
            {
                return $"Could not apply naming suffix on type: {typeSymbol.ToDisplayString()}";
            }
        }
    }

    public static CodeFactory ToConversion(this IEnumerable<string> enumerable)
    {
        return new CodeFactory(enumerable);
    }

    public static CodeFactory ToConversion(this IEnumerable<string> enumerable, TypeIdentifier typeIdentifier)
    {
        return new CodeFactory(enumerable, new[] { typeIdentifier });
    }

    public static INamedTypeSymbol? TryGetNullableValueType(this ITypeSymbol type)
    {
        return type.IsValueType && type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
        } symbol
            ? symbol
            : null;
    }

    public static DynamoDbDataMember[] GetDynamoDbProperties(this ITypeSymbol symbol)
    {
        // A special rule when it comes to Tuples.
        // If we remove this we will get duplicated DataMembers when tuples are being used.
        var items = symbol is INamedTypeSymbol { IsTupleType: true } namedTypeSymbol
            ? namedTypeSymbol.TupleElements.Select(tupleElement => Create(DataMember.FromField(tupleElement)))
                .Where(dynamoDataMember => dynamoDataMember is not null)
                .Select(dynamoDataMember => dynamoDataMember!.Value)
            : symbol
                .GetMembersToObject()
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
                .Select(x => x!.Value);

        return items.ToArray();

        static DynamoDbDataMember? Create(DataMember dataMember)
        {
            var attributes = DynamoDbDataMember.GetDynamoDbAttributes(dataMember.BaseSymbol);
            if (DynamoDbDataMember.IsIgnored(attributes))
                return null;

            return new DynamoDbDataMember(dataMember, attributes);
        }
    }

    public static IEnumerable<ISymbol> GetMembersToObject(this ITypeSymbol typeSymbol)
    {
        // Return ImmutableArray<T> if we can.
        return typeSymbol.BaseType is { SpecialType: SpecialType.System_Object }
            ? typeSymbol.GetMembers()
            : Iterator(typeSymbol);

        static IEnumerable<ISymbol> Iterator(ITypeSymbol typeSymbol)
        {
            var namedTypeSymbol = typeSymbol;

            // We know that we must do this at least once.
            do
            {
                foreach (var member in namedTypeSymbol.GetMembers())
                    yield return member;

                namedTypeSymbol = namedTypeSymbol.BaseType;
            } while (namedTypeSymbol is { SpecialType: not SpecialType.System_Object });
        }
    }


    //Source: https://referencesource.microsoft.com/#mscorlib/system/tuple.cs,49b112811bc359fd,references
    private sealed class TupleComparer : IEqualityComparer<(ITypeSymbol, ITypeSymbol)>
    {
        private readonly IEqualityComparer<ITypeSymbol> _comparer;

        public TupleComparer(IEqualityComparer<ITypeSymbol> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals((ITypeSymbol, ITypeSymbol) x, (ITypeSymbol, ITypeSymbol) y)
        {
            return _comparer.Equals(x.Item1, y.Item1) && _comparer.Equals(x.Item2, y.Item2);
        }

        public int GetHashCode((ITypeSymbol, ITypeSymbol) obj)
        {
            var hashCode1 = obj.Item1 is null ? 0 : _comparer.GetHashCode(obj.Item1);
            var hashCode2 = obj.Item2 is null ? 0 : _comparer.GetHashCode(obj.Item2);
            return CombineHashCodes(hashCode1, hashCode2);
        }

        private static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }
    }
}