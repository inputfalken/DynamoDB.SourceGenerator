using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(
        INamedTypeSymbol entityTypeSymbol,
        INamedTypeSymbol? argumentType,
        string? propertyName
    )
    {
        EntityTypeSymbol = entityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated).TypeIdentifier();
        ArgumentType = argumentType is null
            ? EntityTypeSymbol
            : argumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated).TypeIdentifier();
        AccessName = propertyName ?? $"{EntityTypeSymbol.TypeSymbol.Name}Marshaller";
        ImplementationName = $"{AccessName}Implementation";
    }

    public string ImplementationName { get; }
    public TypeIdentifier EntityTypeSymbol { get; }
    public TypeIdentifier ArgumentType { get; }
    public string AccessName { get; }
}