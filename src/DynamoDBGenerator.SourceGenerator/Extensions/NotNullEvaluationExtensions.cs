using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class NotNullEvaluationExtensions
{
    public static string NotNullTernaryExpression(this ITypeSymbol typeSymbol, in string accessPattern, in string truthy,
        in string falsy)
    {
        var result = Expression(in typeSymbol, in accessPattern) is { } expression
            ? $"{expression} ? {truthy} : {falsy}"
            : truthy;

        return $"({result})";
    }

    public static bool IsNullable(this ITypeSymbol typeSymbol)
    {
        return typeSymbol switch
        {
            { IsReferenceType: true, NullableAnnotation: NullableAnnotation.None or NullableAnnotation.Annotated } => true,
            { IsReferenceType: true, NullableAnnotation: NullableAnnotation.NotAnnotated } => false,
            { IsValueType: true, OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } => true,
            { IsValueType: true } => false,
            _ => throw new ArgumentOutOfRangeException($"Could not determine nullablity of type '{typeSymbol.ToDisplayString()}'.")
        };
    }


    private static string CreateException(in string accessPattern)
    {
        return $"throw {Constants.DynamoDBGenerator.ExceptionHelper.NullExceptionMethod}(nameof({accessPattern}));";
    }

    public static string NotNullIfStatement(this ITypeSymbol typeSymbol, in string accessPattern, in string truthy)
    {
        if (Expression(typeSymbol, accessPattern) is not { } expression)
            return truthy;

        var ifClause = $"if ({expression}) {{ {truthy} }}";
        return typeSymbol.NullableAnnotation switch
        {
            NullableAnnotation.None or NullableAnnotation.Annotated => ifClause,
            NullableAnnotation.NotAnnotated => $"{ifClause} else {{ {CreateException(in accessPattern)} }}",
            _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
        };
    }


    /// <summary>
    ///     If this expression returns null it means that the evaluation determined the expression to be truthy.
    /// </summary>
    private static string? Expression(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        return typeSymbol.IsReferenceType
            ? $"{accessPattern} is not null"
            : OnValueType(in typeSymbol, in accessPattern);

        static string? OnValueType(in ITypeSymbol typeSymbol, in string accessPattern)
        {

            if (typeSymbol.TryGetNullableValueType() is not { } namedTypeSymbol)
                return null;

            var T = namedTypeSymbol.TypeArguments[0];

            var expression = Expression(in T, $"{accessPattern}.Value");

            return expression is null
                ? $"{accessPattern} is not null"
                : $"{accessPattern} is not null && {expression}";
        }
    }


}