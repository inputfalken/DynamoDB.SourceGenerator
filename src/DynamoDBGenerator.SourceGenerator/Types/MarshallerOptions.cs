using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Attribute.DynamoDbMarshallerOptionsArgument;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct MarshallerOptions
{
    private readonly INamedTypeSymbol _convertersType;
    private readonly int _enumStrategy;
    // TODO name needs to take namespace etc into account in order to make it accessible from the outside.
    private const string TypeName = "MarshallerOptions";
    public const string FieldReference = "_options";
    public const string ParamReference = "options";
    private const string ConvertersProperty = "Converters";
    private readonly string _converterFullPath;
    public string FullName { get; }
    public string FieldDeclaration { get; }

    public bool IsUnknown(TypeIdentifier typeIdentifier) => typeIdentifier is UnknownType && IsConvertable(typeIdentifier) is false;
    public bool IsConvertable(TypeIdentifier typeIdentifier) => typeIdentifier.TypeSymbol.TypeKind is TypeKind.Enum || Converters.ContainsKey(typeIdentifier.TypeSymbol);

    private MarshallerOptions(
        INamedTypeSymbol originalType,
        INamedTypeSymbol convertersType,
        IEnumerable<KeyValuePair<string, ITypeSymbol>> converters,
        int enumStrategy
    )
    {
        Converters = converters.ToDictionary(x => x.Value, x => x, SymbolEqualityComparer.Default);
        _convertersType = convertersType;
        _enumStrategy = enumStrategy;
        _converterFullPath = _convertersType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        FullName = $"{originalType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{TypeName}";
        FieldDeclaration = $"private readonly {FullName} {FieldReference};";
    }

    public string? TryInstantiate()
    {
        if (_convertersType.InstanceConstructors.Length is 0 ||
            _convertersType.InstanceConstructors.All(x => x.Parameters.Length is 0))
            return $"new {FullName}(new {_converterFullPath}())";

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
    public string? TryReadConversion(TypeIdentifier typeIdentifier, string attributeValueParam)
    {
        // Converters comes first so that you your customized converters are always prioritized.
        if (Converters.TryGetValue(typeIdentifier.TypeSymbol, out var match))
            return $"{ParamReference}.{ConvertersProperty}.{match.Key}.Read({attributeValueParam})";

        if (typeIdentifier.TypeSymbol.TypeKind is TypeKind.Enum)
        {
            return _enumStrategy switch 
            {
                ConversionStrategy.Integer => $"(Int32.TryParse({attributeValueParam}.N, out var e) ? ({typeIdentifier.UnannotatedString}?) e : null)",
                ConversionStrategy.Name => $"(Enum.TryParse<{typeIdentifier.UnannotatedString}>({attributeValueParam}.S, false, out var e) ? ({typeIdentifier.UnannotatedString}?) e : null)",
                ConversionStrategy.NameCI 
                    or ConversionStrategy.LowerCase 
                    or ConversionStrategy.UpperCase 
                    => $"(Enum.TryParse<{typeIdentifier.UnannotatedString}>({attributeValueParam}.S, true, out var e) ? ({typeIdentifier.UnannotatedString}?) e : null)",
                _ => throw new ArgumentException($"Could not resolve enum conversion strategy from value '{_enumStrategy}'.")
            };
        }
        
        return null;
    }


    private Dictionary<ISymbol?, KeyValuePair<string, ITypeSymbol>> Converters { get; }

    public IEnumerable<string> ClassDeclaration
    {
        get
        {
            var body = $"public {TypeName} ({_converterFullPath} converters)"
                .CreateScope($"{ConvertersProperty} = converters;")
                .Append($"public {_converterFullPath} {ConvertersProperty} {{ get; }}");

            return $"public sealed class {TypeName}".CreateScope(body);
        }
    }

    public static MarshallerOptions Create(INamedTypeSymbol orignalType,INamedTypeSymbol converterTypeSymbol, int enumStrategy)
    {
        var keyValuePairs = converterTypeSymbol
            .GetMembersToObject()
            .Select(ConverterDataMemberOrNull)
            .Where(x => x.HasValue)
            .Select(x => x!.Value);

        return new MarshallerOptions(orignalType, converterTypeSymbol, keyValuePairs, enumStrategy);
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