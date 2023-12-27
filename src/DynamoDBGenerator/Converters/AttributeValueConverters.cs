using System;
using DynamoDBGenerator.Internal.Converters;

namespace DynamoDBGenerator.Converters;

public class AttributeValueConverters
{
    public IAttributeValueConverter<string> StringConverter { get; set; } = new StringConverter();
    public IAttributeValueConverter<DateTime> DateTimeConverter { get; set; } = new ISO8601DateTimeConverter();

    public IAttributeValueConverter<DateTimeOffset> DateTimeOffsetConverter { get; set; } =
        new ISO8601DateTimeOffsetConverter();

    public IAttributeValueConverter<bool> BoolConverter { get; set; } = new BoolConverter();
}