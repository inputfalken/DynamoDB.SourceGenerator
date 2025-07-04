using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class DecimalConverter : IValueTypeConverter<decimal>, IStaticSingleton<DecimalConverter>
{
    public decimal? Read(AttributeValue attributeValue)
    {
        return decimal.TryParse(attributeValue.N, out var @decimal) ? @decimal : null;
    }

    public AttributeValue Write(decimal element)
    {
        return new AttributeValue { N = element.ToString(CultureInfo.InvariantCulture) };
    }
}