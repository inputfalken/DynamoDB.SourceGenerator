using System.Text.RegularExpressions;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeExtensions
{
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

    public static bool IsTemporal(this ITypeSymbol typeSymbol)
    {
        return typeSymbol
            is {SpecialType: SpecialType.System_DateTime}
            or {Name: nameof(DateTimeOffset) or "DateOnly"};
    }
}