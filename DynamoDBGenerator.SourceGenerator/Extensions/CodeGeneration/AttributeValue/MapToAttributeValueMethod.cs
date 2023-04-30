using DynamoDBGenerator.SourceGenerator.Types;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

public readonly record struct MapToAttributeValueMethod(
    in string Code,
    in IEnumerable<Conversion<DynamoDbDataMember, AttributeValueAssignment>> Conversions)
{
    public string Code { get; } = Code;
    public IEnumerable<Conversion<DynamoDbDataMember, AttributeValueAssignment>> Conversions { get; } = Conversions;
};