using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ByteConverter : IValueTypeConverter<byte>, IStaticSingleton<ByteConverter>
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