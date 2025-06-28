using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ShortConverter : IValueTypeConverter<short>, IStaticSingleton<ShortConverter>
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