using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class StringConverter : IReferenceTypeConverter<string>
{
    public string? Read(AttributeValue attributeValue)
    {
        return attributeValue.S;
    }

    public AttributeValue Write(string element)
    {
        return new AttributeValue { S = element };
    }
}