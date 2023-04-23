using Amazon.DynamoDBv2.DataModel;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{
    private const string DynamoDbNameSpace = nameof(Amazon.DynamoDBv2.DataModel);

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
                        ContainingNamespace.Name: DynamoDbNameSpace
                    });
            });
    }

    public static IEnumerable<IPropertySymbol> GetDynamoDbKeys(this INamespaceOrTypeSymbol type)
    {
        return type
            .GetPublicInstanceProperties()
            .Where(x => x.GetAttributes()
                .Any(y => y.AttributeClass is
                    {
                        Name: nameof(DynamoDBHashKeyAttribute),
                        ContainingNamespace.Name: DynamoDbNameSpace
                    }
                    or
                    {
                        Name: nameof(DynamoDBRangeKeyAttribute),
                        ContainingNamespace.Name: DynamoDbNameSpace
                    }
                )
            );
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