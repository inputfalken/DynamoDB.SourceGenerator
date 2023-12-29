using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class LongConverter : IValueTypeConverter<long>
{
    public long? Read(AttributeValue attributeValue)
    {
        return long.TryParse(attributeValue.N, out var @long) ? @long : null;
    }

    public AttributeValue Write(long element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}