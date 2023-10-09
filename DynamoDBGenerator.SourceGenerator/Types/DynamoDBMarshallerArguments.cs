using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(INamedTypeSymbol entityTypeSymbol, INamedTypeSymbol? argumentType, string? propertyName, SymbolEqualityComparer comparer)
    {
        EntityTypeSymbol = (INamedTypeSymbol)entityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ArgumentType = argumentType is null
            ? EntityTypeSymbol
            : (INamedTypeSymbol)argumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        PropertyName = propertyName ?? $"{EntityTypeSymbol.Name}Marshaller";
        ImplementationName = $"{PropertyName}Implementation";
        SymbolComparer = comparer;
    }
    public string ImplementationName { get; }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol ArgumentType { get; }
    public string PropertyName { get; }
    public SymbolEqualityComparer SymbolComparer { get; }

}