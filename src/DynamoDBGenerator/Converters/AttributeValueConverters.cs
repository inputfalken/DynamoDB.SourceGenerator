using System;
using DynamoDBGenerator.Internal.Converters;

namespace DynamoDBGenerator.Converters;

public class AttributeValueConverters
{
    public IReferenceTypeConverter<string> StringConverter { get; set; } = new StringConverter();
    public IValueTypeConverter<DateTime> DateTimeConverter { get; set; } = new ISO8601DateTimeConverter();

    public IValueTypeConverter<DateTimeOffset> DateTimeOffsetConverter { get; set; } =
        new ISO8601DateTimeOffsetConverter();

    public IValueTypeConverter<bool> BoolConverter { get; set; } = new BoolConverter();
}