using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public static class NotNullEvaluation
{
    private const string True = "true";

    public static string TernaryExpression(ITypeSymbol typeSymbol, string accessPattern, string truthy, string falsy)
    {
        var expression = Expression(typeSymbol, accessPattern);
        return expression is True
            ? truthy
            : $"{expression} ? {truthy} : {falsy}";
    }

    public static string? LambdaExpression(ITypeSymbol typeSymbol)
    {
        var expression = Expression(typeSymbol, "x");

        return expression is True ? null : $"x => {expression}";
    }

    private static string Expression(ITypeSymbol typeSymbol, string accessPattern)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return typeSymbol.IsReferenceType ? $"{accessPattern} is not null" : True;

        switch (namedTypeSymbol)
        {
            case {Name: nameof(Nullable)}:
            {
                var T = namedTypeSymbol.TypeArguments[0];

                var expression = Expression(T, $"{accessPattern}.Value");

                return expression is True
                    ? $"{accessPattern}.HasValue"
                    : $"{accessPattern}.HasValue && {expression}";
            }
            case {Name: "KeyValuePair"}:
            {
                var T1 = namedTypeSymbol.TypeArguments[0];
                var T2 = namedTypeSymbol.TypeArguments[1];

                var keyCondition = Expression(T1, $"{accessPattern}.Key");
                var valueCondition = Expression(T2, $"{accessPattern}.Value");
                return (keyCondition, valueCondition) switch
                {
                    (True, True) => True,
                    (True, _) => valueCondition,
                    (_, True) => keyCondition,
                    (_, _) => $"{keyCondition} && {valueCondition}"
                };
            }
        }

        if (namedTypeSymbol.IsValueType)

            throw new NotSupportedException(
                $"Could not determine nullability of '{typeSymbol}' from type '{typeSymbol.OriginalDefinition}' with access pattern '{accessPattern}'."
            );


        return typeSymbol.IsReferenceType ? $"{accessPattern} is not null" : True;
    }

    public static string IfStatement(IPropertySymbol typeSymbol, string truthy)
    {
        return $"if ({Expression(typeSymbol.Type, typeSymbol.Name)}) {{ {truthy} }}";
    }
}