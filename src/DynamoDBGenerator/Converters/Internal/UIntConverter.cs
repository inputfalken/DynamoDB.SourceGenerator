using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class UIntConverter : IValueTypeConverter<uint>, IStaticSingleton<UIntConverter>
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