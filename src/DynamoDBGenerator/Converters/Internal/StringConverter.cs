using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class StringConverter : IReferenceTypeConverter<string>, IStaticSingleton<StringConverter>
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