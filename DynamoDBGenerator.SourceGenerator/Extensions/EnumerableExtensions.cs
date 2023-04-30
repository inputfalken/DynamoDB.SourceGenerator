using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{
    public static ImmutableArray<DynamoDbDataMember> GetDynamoDbProperties(this INamespaceOrTypeSymbol type)
    {
        var res = type
            .GetPublicInstanceProperties()
            .Select(x => new DynamoDbDataMember(x));

        return ImmutableArray.CreateRange(res);
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