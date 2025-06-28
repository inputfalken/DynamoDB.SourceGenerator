using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ISO8601DateOnlyConverter : IValueTypeConverter<DateOnly>, IStaticSingleton<ISO8601DateOnlyConverter>
{
    public DateOnly? Read(AttributeValue attributeValue)
    {
        return DateOnly.TryParse(attributeValue.S, out var date) ? date : null;
    }

    public AttributeValue Write(DateOnly element)
    {
        return new AttributeValue { S = element.ToString("O") };
    }
}