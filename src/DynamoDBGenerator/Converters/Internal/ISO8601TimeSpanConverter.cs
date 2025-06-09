using System;
using System.Xml;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class ISO8601TimeSpanConverter : IValueTypeConverter<TimeSpan>
{
    public TimeSpan? Read(AttributeValue attributeValue)
    {
        var @string = attributeValue.S;
        if (string.IsNullOrWhiteSpace(@string))
            return null;

        if (@string[0] is not 'P')
            return TimeSpan.TryParse(@string, out var timespan) 
                ? timespan 
                : null;
        try
        {
            return XmlConvert.ToTimeSpan(@string);
        }
        catch
        {
            return null;
        }
    }

    public AttributeValue Write(TimeSpan element)
    {
        return new AttributeValue { S = XmlConvert.ToString(element) };
    }
}