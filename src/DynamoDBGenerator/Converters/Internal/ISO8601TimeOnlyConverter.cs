using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal class ISO8601TimeOnlyConverter: IValueTypeConverter<TimeOnly>, IStaticSingleton<ISO8601TimeOnlyConverter>
{
    public TimeOnly? Read(AttributeValue attributeValue)
    {
        return TimeOnly.TryParse(attributeValue.S, out var timeOnly) ? timeOnly : null;
    }

    public AttributeValue Write(TimeOnly element)
    {
        return new AttributeValue {S = element.ToString("O")};
    }
}
