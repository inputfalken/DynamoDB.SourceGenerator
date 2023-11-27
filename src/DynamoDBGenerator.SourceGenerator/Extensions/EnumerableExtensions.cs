using System.Collections.Generic;
using System.Linq;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{

    private static readonly IDictionary<int, string> IndentCache = new Dictionary<int, string>();
    private static string Indent(int level)
    {
        var @base = level switch
        {

            1 => "    ",
            2 => "        ",
            3 => "            ",
            4 => "                ",
            _ => null
        };

        if (@base is not null)
            return @base;

        if (IndentCache.TryGetValue(level, out var indent)) return indent;

        indent = new string(' ', level * 4);
        IndentCache[level] = indent;

        return indent;
    }
    public static IEnumerable<string> CreateBlock(IEnumerable<string> content, int indentLevel, string header)
    {
        var indent = Indent(indentLevel);

        yield return $"{indent}{header}";

        yield return string.Intern($"{indent}{{");

        foreach (var s in content)
            yield return s;

        yield return string.Intern($"{indent}}}");
    }

    public static IReadOnlyList<DynamoDbDataMember> GetDynamoDbProperties(this ITypeSymbol symbol)
    {
        // A special rule when it comes to Tuples.
        // If we remove this we will get duplicated DataMembers when tuples are being used.
        if (symbol is INamedTypeSymbol {IsTupleType: true} namedTypeSymbol)
        {
            var tupleElements = new List<DynamoDbDataMember>(namedTypeSymbol.TupleElements.Length);
            tupleElements.AddRange(namedTypeSymbol.TupleElements.Select(tupleElement => Create(DataMember.FromField(tupleElement)))
                .Where(dynamoDataMember => dynamoDataMember is not null)
                .Select(dynamoDataMember => dynamoDataMember!.Value));

            return tupleElements;
        }

        var members = symbol.GetMembers();
        var list = new List<DynamoDbDataMember>(members.Length);
        if (symbol.BaseType is {SpecialType: not SpecialType.System_Object})
            list.AddRange(GetDynamoDbProperties(symbol.BaseType));

        list.AddRange(members
            .Where(x => x.IsStatic is false)
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .Where(x => x.CanBeReferencedByName)
            .Select(x => x switch
            {
                IPropertySymbol propertySymbol => Create(DataMember.FromProperty(in propertySymbol)),
                IFieldSymbol fieldSymbol => Create(DataMember.FromField(in fieldSymbol)),
                _ => null
            })
            .Where(x => x is not null)
            .Select(x => x!.Value));

        return list;

        static DynamoDbDataMember? Create(DataMember dataMember)
        {

            var attributes = DynamoDbDataMember.GetDynamoDbAttributes(dataMember.BaseSymbol);
            if (DynamoDbDataMember.IsIgnored(attributes))
                return null;

            return new DynamoDbDataMember(dataMember, attributes);
        }

    }
}