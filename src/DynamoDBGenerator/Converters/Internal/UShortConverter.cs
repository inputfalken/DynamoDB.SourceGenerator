using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class UShortConverter : IValueTypeConverter<ushort>
{
    public ushort? Read(AttributeValue attributeValue)
    {
        return ushort.TryParse(attributeValue.N, out var @ushort) ? @ushort : null;
    }

    public AttributeValue Write(ushort element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}