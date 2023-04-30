namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly record struct MapToAttributeValueMethod(
    in string Code,
    in IEnumerable<(AttributeValueInstance AttributeValue, DynamoDbDataMember DataMember)> Mappings)
{
    public string Code { get; } = Code;

    public IEnumerable<(AttributeValueInstance AttributeValue, DynamoDbDataMember DataMember)> Mappings { get; } =
        Mappings;
};