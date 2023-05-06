using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeExtensions
{
    public static string ToXmlComment(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return typeSymbol.ToDisplayString();

        var typeParameters = string.Join(",", namedTypeSymbol.TypeParameters.Select(x => x.Name));

        return Regex.Replace(namedTypeSymbol.ToDisplayString(), "<.+>", $"{{{typeParameters}}}");
    }
}