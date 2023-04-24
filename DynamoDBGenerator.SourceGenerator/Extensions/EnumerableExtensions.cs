using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<DynamoDbProperty> GetDynamoDbProperties(this INamespaceOrTypeSymbol type)
    {
        return type.GetPublicInstanceProperties()
            .Select(x => new DynamoDbProperty(x));
    }
    private static IEnumerable<IPropertySymbol> GetPublicInstanceProperties(this INamespaceOrTypeSymbol symbol)
    {
        return symbol
            .GetMembers()
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Where(x => x is not null)
            .Where(x => x is IPropertySymbol)
            .Cast<IPropertySymbol>();
    }
}