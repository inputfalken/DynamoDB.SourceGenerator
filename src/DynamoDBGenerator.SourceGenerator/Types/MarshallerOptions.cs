using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct MarshallerOptions
{
    private readonly INamedTypeSymbol _convertersType;
    public int EnumStrategy { get; }
    public const string Name = "MarshallerOptions";
    public const string FieldReference = "_options";
    public const string ParamReference = "options";
    private const string ConvertersProperty = "Converters";
    public const string FieldDeclaration = $"private readonly {Name} {FieldReference};";
    private readonly string _converterFullPath;

    private MarshallerOptions(INamedTypeSymbol convertersType,
        IEnumerable<KeyValuePair<string, Converter>> converters, int enumStrategy)
    {
        Converters = converters.ToDictionary(x => x.Value.T, x => x, SymbolEqualityComparer.Default);
        _convertersType = convertersType;
        EnumStrategy = enumStrategy;
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
    

    // TODO move enum conversion in here.
    public string? TryWriteConversion(ITypeSymbol typeSymbol, string elementParam)
    {
        return Converters.TryGetValue(typeSymbol, out var match)
            ? $"{ParamReference}.{ConvertersProperty}.{match.Key}.Write({elementParam})"
            : null;
    }
    public string? TryReadConversion(ITypeSymbol typeSymbol, string attributeValueParam)
    {
        return Converters.TryGetValue(typeSymbol, out var match) 
            ? $"{ParamReference}.{ConvertersProperty}.{match.Key}.Read({attributeValueParam})" 
            : null;
    }

    public bool IsConvertable(ITypeSymbol typeSymbol)
    {
        return typeSymbol.TypeKind is TypeKind.Enum || Converters.ContainsKey(typeSymbol);
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

    public static MarshallerOptions Create(INamedTypeSymbol typeSymbol, int enumStrategy)
    {
        var keyValuePairs = typeSymbol
            .GetMembersToObject()
            .Select(ConverterDataMemberOrNull)
            .Where(x => x.HasValue)
            .Select(x => x!.Value);

        return new MarshallerOptions(typeSymbol, keyValuePairs, enumStrategy);
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