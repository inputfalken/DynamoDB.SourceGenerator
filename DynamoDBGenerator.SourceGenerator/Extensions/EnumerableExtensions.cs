using System.Collections.Immutable;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<DynamoDbDataMember> GetDynamoDbProperties(this INamespaceOrTypeSymbol type)
    {
        return type
            .GetPublicInstanceProperties()
            .Select(x => new DynamoDbDataMember(x));
    }

    private static IEnumerable<DataMember> GetPublicInstanceProperties(this INamespaceOrTypeSymbol symbol)
    {
        var publicInstanceMembers = symbol
            .GetMembers()
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public);

        foreach (var member in publicInstanceMembers)
        {
            switch (member)
            {
                case IPropertySymbol propertySymbol:
                    yield return DataMember.FromProperty(in propertySymbol);
                    break;
                case IFieldSymbol fieldSymbol:
                    yield return DataMember.FromField(in fieldSymbol);
                    break;
            }
        }
    }
}