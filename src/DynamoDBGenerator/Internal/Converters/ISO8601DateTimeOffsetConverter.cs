using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

internal sealed class ISO8601DateTimeOffsetConverter : IAttributeValueConverter<DateTimeOffset>
{
    public DateTimeOffset Read(AttributeValue attributeValue)
    {
        return DateTimeOffset.Parse(attributeValue.S);
    }

    public AttributeValue Write(DateTimeOffset element)
    {
        return new AttributeValue { S = element.ToString("O") };
    }
}