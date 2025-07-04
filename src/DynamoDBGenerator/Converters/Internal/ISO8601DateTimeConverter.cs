using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ISO8601DateTimeConverter : IValueTypeConverter<DateTime>, IStaticSingleton<ISO8601DateTimeConverter>
{
    public DateTime? Read(AttributeValue attributeValue)
    {
        return DateTime.TryParse(attributeValue.S, out var dateTime) ? dateTime : null;
    }

    public AttributeValue Write(DateTime element)
    {
        return new AttributeValue { S = element.ToString("O") };
    }
}