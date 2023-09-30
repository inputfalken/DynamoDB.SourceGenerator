using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(ITypeSymbol entityTypeSymbol, string propertyName, ITypeSymbol argumentType)
    {
        EntityTypeSymbol = (INamedTypeSymbol)entityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ArgumentType = (INamedTypeSymbol)argumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        PropertyName = propertyName;
    }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol ArgumentType { get; }
    public string PropertyName { get; }
}