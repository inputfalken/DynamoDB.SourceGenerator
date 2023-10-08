using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public class DynamoDBMarshallerArguments
{
    public DynamoDBMarshallerArguments(
        ITypeSymbol entityTypeSymbol,
        string? consumerAccessProperty,
        ITypeSymbol argumentType,
        DynamoDBMarshallerArguments? delegation
    )
    {
        EntityTypeSymbol = (INamedTypeSymbol)entityTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ArgumentType = (INamedTypeSymbol)argumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        ConsumerAccessProperty = consumerAccessProperty ?? $"{entityTypeSymbol.Name}Marshaller";
        Delegation = delegation;
        ImplementationName = $"{ConsumerAccessProperty}Implementation";
    }
    public string ImplementationName { get; }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol ArgumentType { get; }
    public string ConsumerAccessProperty { get; }
    public DynamoDBMarshallerArguments? Delegation { get; }

}