using Amazon.DynamoDBv2.DataModel;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public static class EnumerableExtensions
{
    public static IEnumerable<IPropertySymbol> GetDynamoDbProperties(this INamespaceOrTypeSymbol type)
    {
        return type
            .GetPublicInstanceProperties()
            .Where(x =>
            {
                var attributes = x.GetAttributes();
                if (attributes.Length == 0)
                    return true;

                return attributes
                    .Any(y => y.AttributeClass is not
                    {
                        Name: nameof(DynamoDBIgnoreAttribute),
                        ContainingNamespace.Name: nameof(Amazon.DynamoDBv2.DataModel)
                    });
            });
    }

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
}