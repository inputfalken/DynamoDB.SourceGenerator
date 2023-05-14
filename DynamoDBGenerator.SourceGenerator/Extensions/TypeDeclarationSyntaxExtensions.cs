using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeDeclarationSyntaxExtensions
{
    public static IEnumerable<T> GetUnique<T>(this IEnumerable<T> typeDeclarationSyntaxes) where T: TypeDeclarationSyntax
    {
        var set = new HashSet<SyntaxToken>();

        foreach (var typeDeclarationSyntax in typeDeclarationSyntaxes)
        {
            if (set.Add(typeDeclarationSyntax.Identifier))
            {
                yield return typeDeclarationSyntax;
            }
        }
    }

}