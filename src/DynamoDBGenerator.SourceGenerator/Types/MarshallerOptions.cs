using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct MarshallerOptions
{
    private MarshallerOptions(IReadOnlyList<KeyValuePair<string, Converter>> converters)
    {
        Converters = converters;
    }

    public IReadOnlyList<KeyValuePair<string, Converter>> Converters { get; }

    public static MarshallerOptions Create(ITypeSymbol typeSymbol)
    {
        var namedTypeSymbols = typeSymbol.GetMembers();

        var keyValuePairs = namedTypeSymbols
            .Select(ConverterDataMemberOrNull)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();

        return new MarshallerOptions(keyValuePairs);
    }

    private static KeyValuePair<string, Converter>? ConverterDataMemberOrNull(ISymbol symbol)
    {
        // Do not include hidden versions
        if (symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            return null;

        return symbol switch
        {
            IFieldSymbol x when Predicate(x.Type) => new KeyValuePair<string, Converter>(x.Name,
                new Converter(x.Type, (INamedTypeSymbol)x.Type)),
            IPropertySymbol x when Predicate(x.Type) => new KeyValuePair<string, Converter>(x.Name,
                new Converter(x.Type, (INamedTypeSymbol)x.Type)),
            IFieldSymbol x when x.Type.Interfaces.FirstOrDefault(Predicate) is { } y =>
                new KeyValuePair<string, Converter>(x.Name, new Converter(x.Type, y)),
            IPropertySymbol x when x.Type.Interfaces.FirstOrDefault(Predicate) is { } y =>
                new KeyValuePair<string, Converter>(x.Name, new Converter(x.Type, y)),
            _ => null
        };

        static bool Predicate(ITypeSymbol x)
        {
            if (x is not INamedTypeSymbol namedTypeSymbol)
                return false;

            return namedTypeSymbol is
            {
                ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Converters,
                TypeKind: TypeKind.Interface, Name: "IAttributeValueConverter",
                TypeParameters.Length: 1,
                ContainingAssembly.Name : Constants.DynamoDBGenerator.AssemblyName
            };
        }
    }

    public readonly struct Converter
    {
        public Converter(ITypeSymbol originalType, INamedTypeSymbol converterType)
        {
            OriginalType = originalType;
            ConverterType = converterType;
            T = converterType.TypeArguments[0];
            PropertyAccess = OriginalType.Name;
        }


        public ITypeSymbol OriginalType { get; }
        public INamedTypeSymbol ConverterType { get; }
        public ITypeSymbol T { get; }
        public string PropertyAccess { get; }
    }
}