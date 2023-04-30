using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly record struct MapToAttributeValueMethod(
    in string Code,
    in IEnumerable<(AttributeValueAssignment AttributeValue, DynamoDbDataMember DataMember)> Mappings)
{
    public string Code { get; } = Code;

    public IEnumerable<(AttributeValueAssignment AttributeValue, DynamoDbDataMember DataMember)> Mappings { get; } =
        Mappings;
};