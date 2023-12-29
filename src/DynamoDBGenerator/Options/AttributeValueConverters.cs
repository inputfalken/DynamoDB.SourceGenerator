using System;
using System.IO;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Converters.Internal;

namespace DynamoDBGenerator.Options;

/// <summary>
/// Contains the default AttributeValue Converters.
/// You can inherit from this class in order to extend the default behaviour.
/// </summary>
public class AttributeValueConverters
{
    /// <summary>
    /// The <see cref="string"/> converter.
    /// </summary>
    public IReferenceTypeConverter<string> StringConverter { get; set; } = new StringConverter();

    /// <summary>
    /// The <see cref="DateTime"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTime> DateTimeConverter { get; set; } = new ISO8601DateTimeConverter();

    /// <summary>
    /// The <see cref="DateTimeOffsetConverter"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTimeOffset> DateTimeOffsetConverter { get; set; } = new ISO8601DateTimeOffsetConverter();

    /// <summary>
    /// The <see cref="bool"/> converter.
    /// </summary>
    public IValueTypeConverter<bool> BoolConverter { get; set; } = new BoolConverter();

    /// <summary>
    /// The <see cref="char"/> converter.
    /// </summary>
    public IValueTypeConverter<char> CharConverter { get; set; } = new CharConverter();

    /// <summary>
    /// The <see cref="MemoryStream"/> converter.
    /// </summary>
    public IReferenceTypeConverter<MemoryStream> MemoryStreamConverter { get; set; } = new MemoryStreamConverter();

    /// <summary>
    /// The <see cref="int"/> converter.
    /// </summary>
    public IValueTypeConverter<int> Int32Converter { get; set; } = new Int32Converter();

    /// <summary>
    /// The <see cref="decimal"/> converter.
    /// </summary>
    public IValueTypeConverter<decimal> DecimalConverter { get; } = new DecimalConverter();

    /// <summary>
    /// The <see cref="double"/> converter.
    /// </summary>
    public IValueTypeConverter<double> DoubleConverter { get; } = new DoubleConverter();

    /// <summary>
    /// The <see cref="float"/> converter.
    /// </summary>
    public IValueTypeConverter<float> FloatConverter { get; } = new FloatConverter();
}