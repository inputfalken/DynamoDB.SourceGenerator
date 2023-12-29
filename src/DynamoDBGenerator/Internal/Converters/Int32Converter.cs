using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

internal sealed class Int32Converter : IValueTypeConverter<int>
{
    public int? Read(AttributeValue attributeValue)
    {
        return int.TryParse(attributeValue.N, out var @int) ? @int : null;
    }

    public AttributeValue Write(int element)
    {
        return new AttributeValue { N = element.ToString() };
    }
}