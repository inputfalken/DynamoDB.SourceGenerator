using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class FloatConverter : IValueTypeConverter<float>
{
    public float? Read(AttributeValue attributeValue)
    {
        return float.TryParse(attributeValue.N, out var @float) ? @float : null;
    }

    public AttributeValue Write(float element)
    {
        return new AttributeValue { N = element.ToString(CultureInfo.InvariantCulture) };
    }
}