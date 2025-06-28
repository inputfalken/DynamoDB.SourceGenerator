using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ISO8601DateTimeOffsetConverter : IValueTypeConverter<DateTimeOffset>, IStaticSingleton<ISO8601DateTimeOffsetConverter>
{
    public DateTimeOffset? Read(AttributeValue attributeValue)
    {
        return DateTimeOffset.TryParse(attributeValue.S, out var dateTimeOffset) 
            ? dateTimeOffset 
            : null;
    }

    public AttributeValue Write(DateTimeOffset element)
    {
        return new AttributeValue { S = element.ToString("O") };
    }
}