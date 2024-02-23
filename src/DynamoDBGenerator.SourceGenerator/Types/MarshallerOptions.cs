using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Attribute.DynamoDbMarshallerOptionsArgument;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct MarshallerOptions
{
    private readonly INamedTypeSymbol _convertersType;
    private readonly int _enumStrategy;
    public const string Name = "MarshallerOptions";
    public const string FieldReference = "_options";
    public const string ParamReference = "options";
    private const string ConvertersProperty = "Converters";
    public const string FieldDeclaration = $"private readonly {Name} {FieldReference};";
    private readonly string _converterFullPath;

    private MarshallerOptions(INamedTypeSymbol convertersType,
        IEnumerable<KeyValuePair<string, ITypeSymbol>> converters, int enumStrategy)
    {
        Converters = converters.ToDictionary(x => x.Value, x => x, SymbolEqualityComparer.Default);
        _convertersType = convertersType;
        _enumStrategy = enumStrategy;
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
    

    public string? TryWriteConversion(ITypeSymbol typeSymbol, string elementParam)
    {
        // Converters comes first so that you your customized converters are always prioritized.
        if (Converters.TryGetValue(typeSymbol, out var match))
            return $"{ParamReference}.{ConvertersProperty}.{match.Key}.Write({elementParam})";

        if (typeSymbol.TypeKind is TypeKind.Enum)
        {
            return _enumStrategy switch
            {
                ConversionStrategy.Integer => $"new AttributeValue {{ N = ((int){elementParam}).ToString() }}",
                ConversionStrategy.Name => $"new AttributeValue {{ S = {elementParam}.ToString() }}",
                ConversionStrategy.NameCI => $"new AttributeValue {{ S = {elementParam}.ToString() }}",
                ConversionStrategy.LowerCase => $"new AttributeValue {{ S = {elementParam}.ToString().ToLowerInvariant() }}",
                ConversionStrategy.UpperCase => $"new AttributeValue {{ S = {elementParam}.ToString().ToUpperInvariant() }}",
                _ => throw new ArgumentException($"Could not resolve enum conversion strategy from value '{_enumStrategy}'.")
            };
        }
        
        return null;
    }
    public string? TryReadConversion(ITypeSymbol typeSymbol, string attributeValueParam)
    {
        // Converters comes first so that you your customized converters are always prioritized.
        if (Converters.TryGetValue(typeSymbol, out var match))
            return $"{ParamReference}.{ConvertersProperty}.{match.Key}.Read({attributeValueParam})";

        if (typeSymbol.TypeKind is TypeKind.Enum)
        {
            var original = typeSymbol.Representation().original;
            return _enumStrategy switch 
            {
                ConversionStrategy.Integer => $"Int32.TryParse({attributeValueParam}.N, out var @enum) ? ({original}?)@enum : null",
                ConversionStrategy.Name => $"Enum.TryParse<{original}>({attributeValueParam}.S, false, out var @enum) ? ({original}?)@enum : null",
                ConversionStrategy.NameCI 
                    or ConversionStrategy.LowerCase 
                    or ConversionStrategy.UpperCase 
                    => $"Enum.TryParse<{original}>({attributeValueParam}.S, true, out var @enum) ? ({original}?)@enum : null",
                _ => throw new ArgumentException($"Could not resolve enum conversion strategy from value '{_enumStrategy}'.")
            };
        }
        
        return null;
    }

    public bool IsConvertable(ITypeSymbol typeSymbol)
    {
        return typeSymbol.TypeKind is TypeKind.Enum || Converters.ContainsKey(typeSymbol);
    }

    private Dictionary<ISymbol?, KeyValuePair<string, ITypeSymbol>> Converters { get; }

    public IEnumerable<string> ClassDeclaration
    {
        get
        {
            var body = $"public {Name} ({_converterFullPath} converters)"
                .CreateScope($"{ConvertersProperty} = converters;")
                .Append($"public {_converterFullPath} {ConvertersProperty} {{ get; }}");

            return $"public sealed class {Name}".CreateScope(body);
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

    private static KeyValuePair<string, ITypeSymbol>? ConverterDataMemberOrNull(ISymbol symbol)
    {
        return symbol switch
        {
            { DeclaredAccessibility: not (Accessibility.Public or Accessibility.Internal) } => null,
            IFieldSymbol x when Predicate(x.Type) => new KeyValuePair<string, ITypeSymbol>(x.Name, ((INamedTypeSymbol)x.Type).TypeArguments[0]),
            IPropertySymbol x when Predicate(x.Type) => new KeyValuePair<string, ITypeSymbol>(x.Name, ((INamedTypeSymbol)x.Type).TypeArguments[0]),
            IFieldSymbol x when x.Type.Interfaces.FirstOrDefault(Predicate) is { } y => new KeyValuePair<string, ITypeSymbol>(x.Name, y.TypeArguments[0]),
            IPropertySymbol x when x.Type.Interfaces.FirstOrDefault(Predicate) is { } y => new KeyValuePair<string, ITypeSymbol>(x.Name, y.TypeArguments[0]),
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
}