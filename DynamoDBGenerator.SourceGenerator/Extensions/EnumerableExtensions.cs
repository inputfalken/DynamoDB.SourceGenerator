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
        var publicInstanceDataMembers = symbol
            .GetMembers()
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .Where(x => x.CanBeReferencedByName);

        // A special rule when it comes to Tuples.
        // If we remove this we will get duplicated DataMembers when tuples are being used.
        if (symbol is INamedTypeSymbol {IsTupleType: true} namedTypeSymbol)
        {
            foreach (var tupleElement in namedTypeSymbol.TupleElements)
                yield return DataMember.FromField(in tupleElement);

            yield break;
        }

        foreach (var member in publicInstanceDataMembers)
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