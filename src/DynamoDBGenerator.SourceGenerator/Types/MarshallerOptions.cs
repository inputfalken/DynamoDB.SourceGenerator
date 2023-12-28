using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct MarshallerOptions
{
    private readonly INamedTypeSymbol _convertersType;
    public const string Name = "MarshallerOptions";
    public const string PropertyName = "Options";
    private const string ConvertersProperty = "Converters";
    public const string PropertyDeclaration = $"public {Name} {PropertyName} {{ get; }}";

    private MarshallerOptions(INamedTypeSymbol convertersType,
        IEnumerable<KeyValuePair<string, Converter>> converters)
    {
        Converters = converters.ToArray();
        _convertersType = convertersType;
    }

    public string? TryInstantiate()
    {
        if (_convertersType.InstanceConstructors.Length is 0 ||
            _convertersType.InstanceConstructors.All(x => x.Parameters.Length is 0))
        {
            return $"new {Name}(new {_convertersType.Name}())";
        }

        return null;
    }

    public string? AccessConverterWrite(ITypeSymbol typeSymbol, string elementParam)
    {
        var match = Converters
            .Cast<KeyValuePair<string, Converter>?>()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.Value.Value.T, typeSymbol));

        if (match is null)
            return null;

        return $"{PropertyName}.{ConvertersProperty}.{match.Value.Key}.Write({elementParam})";
    }
    public string? AccessConverterRead(ITypeSymbol typeSymbol, string attributeValueParam)
    {
        var match = Converters
            .Cast<KeyValuePair<string, Converter>?>()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.Value.Value.T, typeSymbol));

        if (match is null)
            return null;

        return $"{PropertyName}.{ConvertersProperty}.{match.Value.Key}.Read({attributeValueParam})";
    }

    public bool IsConvertable(ITypeSymbol typeSymbol)
    {
        return Converters.Any(x => SymbolEqualityComparer.Default.Equals(x.Value.T, typeSymbol));
    }

    private KeyValuePair<string, Converter>[] Converters { get; }


    public IEnumerable<string> ClassDeclaration
    {
        get
        {
            var fullPath = _convertersType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var body = $"public {Name} ({fullPath} converters)"
                .CreateBlock($"{ConvertersProperty} = converters;")
                .Append($"public {fullPath} {ConvertersProperty} {{ get; }}");

            return $"public sealed class {Name}".CreateBlock(body);
        }
    }

    public static MarshallerOptions Create(INamedTypeSymbol typeSymbol)
    {
        var keyValuePairs = typeSymbol
            .TraverseToObject()
            .Select(ConverterDataMemberOrNull)
            .Where(x => x.HasValue)
            .Select(x => x!.Value);

        return new MarshallerOptions(typeSymbol, keyValuePairs);
    }

    private static KeyValuePair<string, Converter>? ConverterDataMemberOrNull(ISymbol symbol)
    {
        return symbol switch
        {
            { DeclaredAccessibility: not (Accessibility.Public or Accessibility.Internal) } => null,
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
                TypeKind: TypeKind.Interface, Name: (Constants.DynamoDBGenerator.Converter.ReferenceType or Constants.DynamoDBGenerator.Converter.ValueType),
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