using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

internal sealed class ISO8601DateTimeConverter : IAttributeValueConverter<DateTime>
{
    public DateTime Read(AttributeValue attributeValue)
    {
        return DateTime.Parse(attributeValue.S);
    }

    public AttributeValue Write(DateTime element)
    {
        return new AttributeValue { S = element.ToString("O") };
    }
}