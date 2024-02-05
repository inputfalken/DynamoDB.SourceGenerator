using System;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

public class ISO8601TimeOnlyConverter: IValueTypeConverter<TimeOnly>
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