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
    private readonly string _converterFullPath;

    private MarshallerOptions(INamedTypeSymbol convertersType,
        IEnumerable<KeyValuePair<string, Converter>> converters)
    {
        Converters = converters.ToDictionary(x => x.Value.T, x => x, SymbolEqualityComparer.Default);
        _convertersType = convertersType;
        _converterFullPath = _convertersType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public string? TryInstantiate()
    {
        if (_convertersType.InstanceConstructors.Length is 0 ||
            _convertersType.InstanceConstructors.All(x => x.Parameters.Length is 0))
        {
            return $"new {Name}(new {_converterFullPath}())";
        }

        return null;
    }

    public string? AccessConverterWrite(ITypeSymbol typeSymbol, string elementParam)
    {
        return Converters.TryGetValue(typeSymbol, out var match)
            ? $"{PropertyName}.{ConvertersProperty}.{match.Key}.Write({elementParam})"
            : null;
    }
    public string? AccessConverterRead(ITypeSymbol typeSymbol, string attributeValueParam)
    {
        return Converters.TryGetValue(typeSymbol, out var match) 
            ? $"{PropertyName}.{ConvertersProperty}.{match.Key}.Read({attributeValueParam})" 
            : null;
    }

    public bool IsConvertable(ITypeSymbol typeSymbol)
    {
        return Converters.ContainsKey(typeSymbol);
    }

    private Dictionary<ISymbol?, KeyValuePair<string, Converter>> Converters { get; }


    public IEnumerable<string> ClassDeclaration
    {
        get
        {
            var body = $"public {Name} ({_converterFullPath} converters)"
                .CreateBlock($"{ConvertersProperty} = converters;")
                .Append($"public {_converterFullPath} {ConvertersProperty} {{ get; }}");

            return $"public sealed class {Name}".CreateBlock(body);
        }
    }

    public static MarshallerOptions Create(INamedTypeSymbol typeSymbol)
    {
        var keyValuePairs = typeSymbol
            .GetMembersToObject()
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
            IFieldSymbol x when Predicate(x.Type) => new KeyValuePair<string, Converter>(x.Name, new Converter(x.Type, (INamedTypeSymbol)x.Type)),
            IPropertySymbol x when Predicate(x.Type) => new KeyValuePair<string, Converter>(x.Name, new Converter(x.Type, (INamedTypeSymbol)x.Type)),
            IFieldSymbol x when x.Type.Interfaces.FirstOrDefault(Predicate) is { } y => new KeyValuePair<string, Converter>(x.Name, new Converter(x.Type, y)),
            IPropertySymbol x when x.Type.Interfaces.FirstOrDefault(Predicate) is { } y => new KeyValuePair<string, Converter>(x.Name, new Converter(x.Type, y)),
            _ => null
        };

        static bool Predicate(ITypeSymbol x)
        {
            if (x is not INamedTypeSymbol namedTypeSymbol)
                return false;

            return namedTypeSymbol is
            {
                ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Converters,
                TypeKind: TypeKind.Interface, Name: Constants.DynamoDBGenerator.Converter.ReferenceType or Constants.DynamoDBGenerator.Converter.ValueType,
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