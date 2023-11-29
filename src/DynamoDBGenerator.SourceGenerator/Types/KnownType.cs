using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public abstract record TypeIdentifier;

public record UnknownType(ITypeSymbol TypeSymbol) : TypeIdentifier
{
    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;

}

public record KeyValueGeneric : TypeIdentifier
{
    public SupportedType Type { get; }

    // ReSharper disable once InconsistentNaming
    public ITypeSymbol TKey { get; }

    // ReSharper disable once InconsistentNaming
    public ITypeSymbol TValue { get; }

    private KeyValueGeneric(in ITypeSymbol tKey, in ITypeSymbol tValue, in SupportedType supportedType)
    {
        Type = supportedType;
        TKey = tKey;
        TValue = tValue;
    }

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
            : new KeyValueGeneric(type.TypeArguments[0], type.TypeArguments[1], supported.Value);
    }

    public enum SupportedType
    {
        LookUp = 1,
        Dictionary = 2
    }
}

public record SingleGeneric : TypeIdentifier
{
    public SupportedType Type { get; }
    public ITypeSymbol T { get; }

    private SingleGeneric(ITypeSymbol innerType, in SupportedType supportedType)

    {
        Type = supportedType;
        T = innerType;
    }


    public static SingleGeneric? CreateInstance(in ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            return new SingleGeneric(arrayTypeSymbol.ElementType, SupportedType.Array);

        if (typeSymbol is not INamedTypeSymbol type)
            return null;

        if (type is not {IsGenericType: true, TypeArguments.Length: 1})
            return null;

        SupportedType? supported = type switch
        {
            _ when type.TryGetNullableValueType() is not null => SupportedType.Nullable,
            {Name: "ISet"} => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => SupportedType.Set,
            {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T} => SupportedType.ICollection,
            _ when type.AllInterfaces.Any(x => x is {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T}) => SupportedType.ICollection,
            {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T} => SupportedType.IReadOnlyCollection,
            _ when type.AllInterfaces.Any(x => x is {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T}) => SupportedType.IReadOnlyCollection,
            {OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T} => SupportedType.IEnumerable,
            _ => null
        };

        return supported is null ? null : new SingleGeneric(type.TypeArguments[0], supported.Value);
    }

    public enum SupportedType
    {
        Nullable = 1,
        Set = 2,
        Array = 3,
        ICollection = 4,
        IReadOnlyCollection = 5,
        IEnumerable = 6
    }
}

public record BaseType : TypeIdentifier
{
    public SupportedType Type { get; }

    private BaseType( in SupportedType type)
    {
        Type = type;
    }

    public static BaseType? CreateInstance(in ITypeSymbol type)
    {
        SupportedType? primitiveTypeAssignment = type switch
        {
            {SpecialType: SpecialType.System_String} => SupportedType.String,
            {SpecialType: SpecialType.System_Boolean} => SupportedType.Bool,
            {SpecialType: SpecialType.System_Char} => SupportedType.Char,
            {SpecialType: SpecialType.System_Int16} => SupportedType.Int16,
            {SpecialType: SpecialType.System_Int32} => SupportedType.Int32,
            {SpecialType: SpecialType.System_Int64} => SupportedType.Int64,
            {SpecialType: SpecialType.System_UInt16} => SupportedType.UInt16,
            {SpecialType: SpecialType.System_UInt32} => SupportedType.UInt32,
            {SpecialType: SpecialType.System_UInt64} => SupportedType.UInt64,
            {SpecialType: SpecialType.System_Decimal} => SupportedType.Decimal,
            {SpecialType: SpecialType.System_Double} => SupportedType.Double,
            {SpecialType: SpecialType.System_Single} => SupportedType.Single,
            {SpecialType: SpecialType.System_Byte} => SupportedType.Byte,
            {SpecialType: SpecialType.System_SByte} => SupportedType.SByte,
            {Name: nameof(MemoryStream)} => SupportedType.MemoryStream,
            {TypeKind: TypeKind.Enum} => SupportedType.Enum,
            {SpecialType: SpecialType.System_DateTime} => SupportedType.DateTime,
            {Name: nameof(DateTimeOffset)} => SupportedType.DateTimeOffset,
            {Name: "DateOnly"} => SupportedType.DateOnly,
            _ => null
        };

        return primitiveTypeAssignment is null ? null : new BaseType(primitiveTypeAssignment.Value);
    }

    public enum SupportedType
    {
        String = 1,
        Bool = 2,
        Char = 3,
        Enum = 4,
        Int16 = 5,
        Byte = 6,
        Int32 = 7,
        Int64 = 8,
        SByte = 9,
        UInt16 = 10,
        UInt32 = 11,
        UInt64 = 12,
        Decimal = 13,
        Double = 14,
        Single = 15,
        DateTime = 16,
        DateTimeOffset = 17,
        DateOnly = 18,
        MemoryStream = 19
    }
}