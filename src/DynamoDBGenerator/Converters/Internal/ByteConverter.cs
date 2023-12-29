using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ByteConverter : IValueTypeConverter<byte>
{
    public byte? Read(AttributeValue attributeValue)
    {
        return byte.TryParse(attributeValue.N, out var @byte) ? @byte : null;
    }

    public AttributeValue Write(byte element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}