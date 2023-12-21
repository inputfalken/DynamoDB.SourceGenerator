using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(INamedTypeSymbol entityTypeSymbol, INamedTypeSymbol? argumentType, string? propertyName)
    {
        EntityTypeSymbol = (INamedTypeSymbol)entityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ArgumentType = argumentType is null
            ? EntityTypeSymbol
            : (INamedTypeSymbol)argumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        PropertyName = propertyName ?? $"{EntityTypeSymbol.Name}Marshaller";
        ImplementationName = $"{PropertyName}Implementation";

        AnnotatedEntityType = EntityTypeSymbol.Representation().annotated;
        AnnotatedArgumentType = argumentType is null ? AnnotatedEntityType : ArgumentType.Representation().annotated;

    }
    public string ImplementationName { get; }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol ArgumentType { get; }
    public string PropertyName { get; }

    public string AnnotatedEntityType { get; }
    public string AnnotatedArgumentType { get; }

}