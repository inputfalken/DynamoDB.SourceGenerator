using DynamoDBGenerator.SourceGenerator.Types;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

public readonly record struct MapToAttributeValueMethod(
    in string Code,
    in string MethodName,
    in IEnumerable<Conversion<DynamoDbDataMember, AttributeValueAssignment>> Conversions)
{
    public string MethodName { get; } = MethodName;

    /// <summary>
    /// The C# code for the <see cref="AttributeValue"/> conversion.
    /// </summary>
    public string Code { get; } = Code;

    /// <summary>
    /// The conversions that occur within the method.
    /// </summary>
    public IEnumerable<Conversion<DynamoDbDataMember, AttributeValueAssignment>> Conversions { get; } = Conversions;
};