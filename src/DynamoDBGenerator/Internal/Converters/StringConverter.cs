using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

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