using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<string> CreateBlock(int indentLevel, IEnumerable<string> content)
    {
        var indent = StringExtensions.Indent(indentLevel);
        yield return $"{indent}{{";
        
        foreach (var s in content)
            yield return s;
        yield return $"{indent}}}";

    }
    public static IEnumerable<DynamoDbDataMember> GetDynamoDbProperties(this ITypeSymbol type)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var publicInstanceProperty in type.GetPublicInstanceProperties())
        {

            var attributes = DynamoDbDataMember.GetDynamoDbAttributes(publicInstanceProperty.BaseSymbol);
            if (DynamoDbDataMember.IsIgnored(attributes))
                continue;

            yield return new DynamoDbDataMember(publicInstanceProperty, attributes);
        }
    }

    private static IEnumerable<DataMember> GetPublicInstanceProperties(this ITypeSymbol symbol)
    {

        if (symbol.BaseType is {SpecialType: not SpecialType.System_Object})
            foreach (var publicInstanceProperty in GetPublicInstanceProperties(symbol.BaseType))
                yield return publicInstanceProperty;

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