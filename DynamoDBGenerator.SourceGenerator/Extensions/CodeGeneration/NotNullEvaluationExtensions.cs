using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public static class NotNullEvaluationExtensions
{
    public static string TernaryExpression(this IPropertySymbol typeSymbol, string truthy, string falsy)
    {
        return Expression(typeSymbol.Type, typeSymbol.Name) is { } expression
            ? $"{expression} ? {truthy} : {falsy}"
            : truthy;
    }

    public static string? LambdaExpression(this ITypeSymbol typeSymbol)
    {
        return Expression(typeSymbol, "x") is { } expression
            ? $"x => {expression}"
            : null;
    }


    public static string IfStatement(this IPropertySymbol typeSymbol, string truthy)
    {
        return Expression(typeSymbol.Type, typeSymbol.Name) is { } expression
            ? $"if ({expression}) {{ {truthy} }}"
            : truthy;
    }

    /// <summary>
    /// If this expression returns null it means that the evaluation determined the expression to be truthy.
    /// </summary>
    private static string? Expression(ITypeSymbol typeSymbol, string accessPattern)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return typeSymbol.IsReferenceType ? $"{accessPattern} is not null" : null;

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
            case {Name: "KeyValuePair"}:
            {
                return (
                        keyCondition: Expression(namedTypeSymbol.TypeArguments[0], $"{accessPattern}.Key"),
                        valueCondition: Expression(namedTypeSymbol.TypeArguments[1], $"{accessPattern}.Value")
                    ) switch
                    {
                        (null, null) => null,
                        (null, var right) => right,
                        (var left, null) => left,
                        var (left, right) => $"{left} && {right}"
                    };
            }
        }

        if (namedTypeSymbol.IsValueType)
            throw new NotSupportedException(
                $"Could not determine nullability of '{typeSymbol}' from type '{typeSymbol.OriginalDefinition}' with access pattern '{accessPattern}'."
            );

        return typeSymbol.IsReferenceType ? $"{accessPattern} is not null" : null;
    }
}