using System.Collections;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public static class NotNullEvaluationExtensions
{
    public static string TernaryExpression(this ITypeSymbol typeSymbol, in string accessPattern, in string truthy,
        in string falsy)
    {
        var result = Expression(in typeSymbol, in accessPattern) is { } expression
            ? $"{expression} ? {truthy} : {falsy}"
            : truthy;

        return $"({result})";
    }

    public static string? LambdaExpression(this ITypeSymbol typeSymbol)
    {
        return Expression(in typeSymbol, "x") is { } expression
            ? $"x => {expression}"
            : null;
    }


    public static string IfStatement(this DataMember typeSymbol, in string accessPattern, in string truthy)
    {
        return Expression(typeSymbol.Type, accessPattern) is { } expression
            ? $"if ({expression}) {{ {truthy} }}"
            : truthy;
    }

    /// <summary>
    /// If this expression returns null it means that the evaluation determined the expression to be truthy.
    /// </summary>
    private static string? Expression(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        return typeSymbol.IsReferenceType
            ? OnReferenceType(in typeSymbol, in accessPattern)
            : OnValueType(in typeSymbol, in accessPattern);
    }

    private static string? OnValueType(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return null;

        switch (namedTypeSymbol)
        {
            case {Name: nameof(Nullable)}:
            {
                var T = namedTypeSymbol.TypeArguments[0];

                var expression = Expression(in T, $"{accessPattern}.Value");

                return expression is null
                    ? $"{accessPattern}.HasValue"
                    : $"{accessPattern}.HasValue && {expression}";
            }
            case {IsTupleType: true}:
                throw new NotSupportedException($"Null evaluation for type '{typeSymbol}'.");
            default:
                return null;
        }
    }

    private static string? OnReferenceType(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        return $"{accessPattern} is not null";
    }
}