using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class SByteConverter : IValueTypeConverter<sbyte>
{
    public sbyte? Read(AttributeValue attributeValue)
    {
        return sbyte.TryParse(attributeValue.N, out var @sbyte) ? @sbyte : null;
    }

    public AttributeValue Write(sbyte element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}