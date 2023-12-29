using System;
using System.IO;
using DynamoDBGenerator.Internal.Converters;

namespace DynamoDBGenerator.Converters;

public class AttributeValueConverters
{
    public IReferenceTypeConverter<string> StringConverter { get; set; } = new StringConverter();
    public IValueTypeConverter<DateTime> DateTimeConverter { get; set; } = new ISO8601DateTimeConverter();
    public IValueTypeConverter<DateTimeOffset> DateTimeOffsetConverter { get; set; } = new ISO8601DateTimeOffsetConverter();
    public IValueTypeConverter<bool> BoolConverter { get; set; } = new BoolConverter();
    public IValueTypeConverter<char> CharConverter { get; set; } = new CharConverter();
    public IReferenceTypeConverter<MemoryStream> MemoryStreamConverter { get; set; } = new MemoryStreamConverter();
    public IValueTypeConverter<int> Int32Converter { get; set; } = new Int32Converter();
}