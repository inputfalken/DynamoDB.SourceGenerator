using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class UIntConverter : IValueTypeConverter<uint>
{
    public uint? Read(AttributeValue attributeValue)
    {
        return uint.TryParse(attributeValue.N, out var @uint) ? @uint : null;
    }

    public AttributeValue Write(uint element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}