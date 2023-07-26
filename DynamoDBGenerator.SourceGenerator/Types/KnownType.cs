using DynamoDBGenerator.SourceGenerator.Extensions;
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
            {Name: "IGrouping"} => SupportedType.Grouping,
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
        Grouping = 2,
        Dictionary = 3
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
            return new SingleGeneric(arrayTypeSymbol.ElementType, SupportedType.Collection);

        if (typeSymbol is not INamedTypeSymbol type)
            return null;

        if (type is not {IsGenericType: true, TypeArguments.Length: 1})
            return null;

        SupportedType? supported = type switch
        {
            {Name: "Nullable"} => SupportedType.Nullable,
            {Name: "ISet"} => SupportedType.Set,
            {Name: "IEnumerable"} => SupportedType.Collection,
            _ when type.AllInterfaces.Any(x => x is {Name: "ISet"}) => SupportedType.Set,
            _ when type.AllInterfaces.Any(x => x is {Name: "IEnumerable"}) => SupportedType.Collection,
            _ => null
        };
        
        return supported is null ? null : new SingleGeneric(type.TypeArguments[0], supported.Value);
    }

    public enum SupportedType
    {
        Nullable = 1,
        Set = 2,
        Collection = 3
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
            {TypeKind: TypeKind.Enum} => SupportedType.Enum,
            _ when type.IsNumeric() => SupportedType.Number,
            _ when type.IsTemporal() => SupportedType.Temporal,
            _ => null
        };

        return primitiveTypeAssignment is null ? null : new BaseType(in type, primitiveTypeAssignment.Value);
    }

    public enum SupportedType
    {
        String = 1,
        Bool = 2,
        Char = 3,
        Temporal = 4,
        Number = 5,
        Enum = 6
    }
}