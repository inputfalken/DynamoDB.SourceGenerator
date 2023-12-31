using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ULongConverter : IValueTypeConverter<ulong>
{
    public ulong? Read(AttributeValue attributeValue)
    {
        return ulong.TryParse(attributeValue.N, out var @ulong) ? @ulong : null;
    }

    public AttributeValue Write(ulong element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}