using System;
using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class DoubleConverter : IValueTypeConverter<double>
{
    public double? Read(AttributeValue attributeValue)
    {
        return double.TryParse(attributeValue.N, out var @double) ? @double : null;
    }

    public AttributeValue Write(double element)
    {
        return new AttributeValue { N = element.ToString(CultureInfo.InvariantCulture) };
    }
}