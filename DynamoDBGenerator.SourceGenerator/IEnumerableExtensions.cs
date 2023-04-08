using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public static class EnumerableExtensions
{
    public static IEnumerable<IPropertySymbol> GetPublicInstanceProperties(this INamespaceOrTypeSymbol symbol)
    {
        return symbol
            .GetMembers()
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Where(x => x is not null)
            .Where(x => x is IPropertySymbol)
            .Cast<IPropertySymbol>();
    }

    public static IEnumerable<IMethodSymbol> GetPublicInstanceMethods(this INamespaceOrTypeSymbol symbol)
    {
        return symbol
            .GetMembers()
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Where(x => x is not null)
            .Where(x => x is IMethodSymbol)
            .Cast<IMethodSymbol>();
    }
}