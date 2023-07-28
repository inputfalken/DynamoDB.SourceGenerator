using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public abstract record KnownType;

public record KeyValueGeneric : KnownType
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

public record SingleGeneric : KnownType
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
            {Name: "Nullable"} => SupportedType.Nullable,
            {Name: "ISet"} => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => SupportedType.Set,
            {Name:"ICollection"} => SupportedType.ICollection,
            _ when type.AllInterfaces.Any(x => x is {Name: "ICollection"}) => SupportedType.ICollection,
            {Name:"IReadOnlyCollection"} => SupportedType.IReadOnlyCollection,
            _ when type.AllInterfaces.Any(x => x is {Name: "IReadOnlyCollection"}) => SupportedType.IReadOnlyCollection,
            {Name: "IEnumerable"} => SupportedType.IEnumerable,
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

public record BaseType : KnownType
{
    public SupportedType Type { get; }
    private readonly ITypeSymbol _typeSymbol;

    private BaseType(in ITypeSymbol typeSymbol, in SupportedType type)
    {
        Type = type;
        _typeSymbol = typeSymbol;
    }

    public static BaseType? CreateInstance(in ITypeSymbol type)
    {
        SupportedType? primitiveTypeAssignment = type switch
        {
            {SpecialType: SpecialType.System_String} => SupportedType.String,
            {SpecialType: SpecialType.System_Boolean} => SupportedType.Bool,
            {SpecialType: SpecialType.System_Char} => SupportedType.Char,
            {SpecialType: SpecialType.System_Int16} => SupportedType.System_Int16,
            {SpecialType: SpecialType.System_Int32} => SupportedType.System_Int32,
            {SpecialType: SpecialType.System_Int64} => SupportedType.System_Int64,
            {SpecialType: SpecialType.System_UInt16} => SupportedType.System_UInt16,
            {SpecialType: SpecialType.System_UInt32} => SupportedType.System_UInt32,
            {SpecialType: SpecialType.System_UInt64} => SupportedType.System_UInt64,
            {SpecialType: SpecialType.System_Decimal} => SupportedType.System_Decimal,
            {SpecialType: SpecialType.System_Double} => SupportedType.System_Double,
            {SpecialType: SpecialType.System_Single} => SupportedType.System_Single,
            {SpecialType: SpecialType.System_Byte} => SupportedType.System_Byte,
            {SpecialType: SpecialType.System_SByte} => SupportedType.System_SByte,
            {TypeKind: TypeKind.Enum} => SupportedType.Enum,
            {SpecialType: SpecialType.System_DateTime} => SupportedType.System_DateTime,
            {Name:nameof(DateTimeOffset)} => SupportedType.System_DateTimeOffset,
            {Name:"DateOnly"} => SupportedType.System_DateOnly,
            _ => null
        };

        return primitiveTypeAssignment is null ? null : new BaseType(in type, primitiveTypeAssignment.Value);
    }

    public enum SupportedType
    {
        String = 1,
        Bool = 2,
        Char = 3,
        Enum = 4,
        System_Int16 = 5,
        System_Byte = 6,
        System_Int32 = 7,
        System_Int64 = 8,
        System_SByte = 9,
        System_UInt16 = 10,
        System_UInt32 = 11,
        System_UInt64 = 12,
        System_Decimal = 13,
        System_Double = 14,
        System_Single = 15,
        System_DateTime = 16,
        System_DateTimeOffset = 17,
        System_DateOnly = 18
    }
}