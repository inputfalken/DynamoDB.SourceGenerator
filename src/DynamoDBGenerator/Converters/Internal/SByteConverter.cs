using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class SByteConverter : IValueTypeConverter<sbyte>, IStaticSingleton<SByteConverter>
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