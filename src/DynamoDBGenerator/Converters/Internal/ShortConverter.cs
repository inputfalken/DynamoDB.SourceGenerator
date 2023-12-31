using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ShortConverter : IValueTypeConverter<short>
{
    public short? Read(AttributeValue attributeValue)
    {
        return short.TryParse(attributeValue.N, out var @short) ? @short : null;
    }

    public AttributeValue Write(short element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}