using System.Collections;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public static class NotNullEvaluationExtensions
{
    public static string TernaryExpression(this ITypeSymbol typeSymbol, string accessPattern, string truthy,
        string falsy)
    {
        var result = Expression(typeSymbol, accessPattern) is { } expression
            ? $"{expression} ? {truthy} : {falsy}"
            : truthy;

        return $"({result})";
    }

    public static string? LambdaExpression(this ITypeSymbol typeSymbol)
    {
        return Expression(typeSymbol, "x") is { } expression
            ? $"x => {expression}"
            : null;
    }


    public static string IfStatement(this DataMember typeSymbol, string accessPattern, string truthy)
    {
        return Expression(typeSymbol.Type, accessPattern) is { } expression
            ? $"if ({expression}) {{ {truthy} }}"
            : truthy;
    }

    /// <summary>
    /// If this expression returns null it means that the evaluation determined the expression to be truthy.
    /// </summary>
    private static string? Expression(ITypeSymbol typeSymbol, string accessPattern)
    {
        return typeSymbol.IsReferenceType
            ? OnReferenceType(typeSymbol, accessPattern)
            : OnValueType(typeSymbol, accessPattern);
    }

    private static string? OnValueType(ITypeSymbol typeSymbol, string accessPattern)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return null;

        switch (namedTypeSymbol)
        {
            case {Name: nameof(Nullable)}:
            {
                var T = namedTypeSymbol.TypeArguments[0];

                var expression = Expression(T, $"{accessPattern}.Value");

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

    private static string? OnReferenceType(ITypeSymbol typeSymbol, string accessPattern)
    {
        return $"{accessPattern} is not null";
    }
}