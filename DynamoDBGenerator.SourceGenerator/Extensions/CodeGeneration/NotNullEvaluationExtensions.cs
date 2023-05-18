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

    // TODO could be good to pass the type that has the property in order to give an even better exception message.
    private static string CreateException(in ITypeSymbol typeSymbol, string accessPattern)
    {
        return @$"throw new ArgumentNullException(nameof({accessPattern}), ""The property is not supposed to be null, to allow this; make the property nullable.\nLocation:{accessPattern}\nType:{typeSymbol.ToDisplayString()}"");";
    }
    public static string IfStatement(this ITypeSymbol typeSymbol, in string accessPattern, in string truthy)
    {
        if (Expression(typeSymbol, accessPattern) is not { } expression)
            return truthy;

        var ifClause = $"if ({expression}) {{ {truthy} }}";
        return typeSymbol.NullableAnnotation switch
        {
            NullableAnnotation.None => ifClause,
            NullableAnnotation.NotAnnotated => $@"{ifClause} else {{ {CreateException(typeSymbol, accessPattern) }}}",
            NullableAnnotation.Annotated => ifClause,
            _ => throw new ArgumentOutOfRangeException()
        };

    }

    /// <summary>
    ///     If this expression returns null it means that the evaluation determined the expression to be truthy.
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
            default:
                return null;
        }
    }

    private static string? OnReferenceType(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        return $"{accessPattern} is not null";
    }
}