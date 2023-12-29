using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public abstract record TypeIdentifier(ITypeSymbol TypeSymbol)
{
    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;
}

public record UnknownType(ITypeSymbol TypeSymbol) : TypeIdentifier(TypeSymbol);

public record KeyValueGeneric : TypeIdentifier
{

    public enum SupportedType
    {
        LookUp = 1,
        Dictionary = 2
    }

    private KeyValueGeneric(in ITypeSymbol typeSymbol, in ITypeSymbol tKey, in ITypeSymbol tValue, in SupportedType supportedType) : base(typeSymbol)
    {
        Type = supportedType;
        TKey = tKey;
        TValue = tValue;
    }
    public SupportedType Type { get; }

    // ReSharper disable once InconsistentNaming
    public ITypeSymbol TKey { get; }

    // ReSharper disable once InconsistentNaming
    public ITypeSymbol TValue { get; }

    public static KeyValueGeneric? CreateInstance(in ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol type)
            return null;

        if (type is not {IsGenericType: true, TypeArguments.Length: 2})
            return null;

        SupportedType? supported = type switch
        {
            {Name: "ILookup"} => SupportedType.LookUp,
            {Name: "Dictionary" or "IReadOnlyDictionary" or "IDictionary"} => SupportedType.Dictionary,
            _ => null
        };
        return supported is null
            ? null
            : new KeyValueGeneric(typeSymbol, type.TypeArguments[0], type.TypeArguments[1], supported.Value);
    }
}

public record SingleGeneric : TypeIdentifier
{

    public enum SupportedType
    {
        Nullable = 1,
        Set = 2,
        Array = 3,
        ICollection = 4,
        IReadOnlyCollection = 5,
        IEnumerable = 6
    }

    private SingleGeneric(ITypeSymbol type, ITypeSymbol innerType, in SupportedType supportedType) : base(type)

    {
        Type = supportedType;
        T = innerType;
    }
    public SupportedType Type { get; }
    public ITypeSymbol T { get; }


    public static SingleGeneric? CreateInstance(in ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            return new SingleGeneric(typeSymbol, arrayTypeSymbol.ElementType, SupportedType.Array);

        if (typeSymbol is not INamedTypeSymbol type)
            return null;

        if (type is not {IsGenericType: true, TypeArguments.Length: 1})
            return null;

        SupportedType? supported = type switch
        {
            _ when type.TryGetNullableValueType() is not null => SupportedType.Nullable,
            {Name: "ISet" } => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => SupportedType.Set,
            {Name: "IReadOnlySet" } => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is {Name: "IReadOnlySet"}) => SupportedType.Set,
            {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T} => SupportedType.ICollection,
            _ when type.AllInterfaces.Any(x => x is {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T}) => SupportedType.ICollection,
            {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T} => SupportedType.IReadOnlyCollection,
            _ when type.AllInterfaces.Any(x => x is {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T}) => SupportedType.IReadOnlyCollection,
            {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T} => SupportedType.IEnumerable,
            _ => null
        };

        return supported is null ? null : new SingleGeneric(type, type.TypeArguments[0], supported.Value);
    }
}

public record BaseType : TypeIdentifier
{

    public enum SupportedType
    {
        Enum = 4,
        Int16 = 5,
        Byte = 6,
        Int64 = 8,
        SByte = 9,
        UInt16 = 10,
        UInt32 = 11,
        UInt64 = 12,
        Decimal = 13,
        Double = 14,
        Single = 15,
        DateOnly = 18
    }

    private BaseType(ITypeSymbol typeSymbol, in SupportedType type) : base(typeSymbol)
    {
        Type = type;
    }
    public SupportedType Type { get; }

    public static BaseType? CreateInstance(in ITypeSymbol type)
    {
        SupportedType? primitiveTypeAssignment = type switch
        {
            {SpecialType: SpecialType.System_Int16} => SupportedType.Int16,
            {SpecialType: SpecialType.System_Int64} => SupportedType.Int64,
            {SpecialType: SpecialType.System_UInt16} => SupportedType.UInt16,
            {SpecialType: SpecialType.System_UInt32} => SupportedType.UInt32,
            {SpecialType: SpecialType.System_UInt64} => SupportedType.UInt64,
            {SpecialType: SpecialType.System_Decimal} => SupportedType.Decimal,
            {SpecialType: SpecialType.System_Double} => SupportedType.Double,
            {SpecialType: SpecialType.System_Single} => SupportedType.Single,
            {SpecialType: SpecialType.System_Byte} => SupportedType.Byte,
            {SpecialType: SpecialType.System_SByte} => SupportedType.SByte,
            {TypeKind: TypeKind.Enum} => SupportedType.Enum,
            {Name: "DateOnly"} => SupportedType.DateOnly,
            _ => null
        };

        return primitiveTypeAssignment is null ? null : new BaseType(type, primitiveTypeAssignment.Value);
    }
}