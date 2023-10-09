using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(INamedTypeSymbol entityTypeSymbol, INamedTypeSymbol? argumentType, string? consumerAccessProperty, SymbolEqualityComparer comparer)
    {
        EntityTypeSymbol = (INamedTypeSymbol)entityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ArgumentType = argumentType is null
            ? EntityTypeSymbol
            : (INamedTypeSymbol)argumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ConsumerAccessProperty = consumerAccessProperty ?? $"{entityTypeSymbol.Name}Marshaller";
        ImplementationName = $"{ConsumerAccessProperty}Implementation";
        SymbolComparer = comparer;
    }
    public string ImplementationName { get; }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol ArgumentType { get; }
    public string ConsumerAccessProperty { get; }
    public SymbolEqualityComparer SymbolComparer { get; }

}