using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(INamedTypeSymbol entityTypeSymbol, string propertyName, INamedTypeSymbol argumentType)
    {
        EntityTypeSymbol = entityTypeSymbol;
        PropertyName = propertyName;
        ArgumentType = argumentType;
    }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol ArgumentType { get; }
    public string PropertyName { get; }
}