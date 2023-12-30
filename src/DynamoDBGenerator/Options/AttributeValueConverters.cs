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
    public IReferenceTypeConverter<string> StringConverter { get; } = new StringConverter();

    /// <summary>
    /// The <see cref="DateTime"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTime> DateTimeConverter { get; } = new ISO8601DateTimeConverter();

    /// <summary>
    /// The <see cref="DateTimeOffsetConverter"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTimeOffset> DateTimeOffsetConverter { get; } = new ISO8601DateTimeOffsetConverter();

    /// <summary>
    /// The <see cref="bool"/> converter.
    /// </summary>
    public IValueTypeConverter<bool> BoolConverter { get; } = new BoolConverter();

    /// <summary>
    /// The <see cref="char"/> converter.
    /// </summary>
    public IValueTypeConverter<char> CharConverter { get; } = new CharConverter();

    /// <summary>
    /// The <see cref="MemoryStream"/> converter.
    /// </summary>
    public IReferenceTypeConverter<MemoryStream> MemoryStreamConverter { get; } = new MemoryStreamConverter();

    /// <summary>
    /// The <see cref="int"/> converter.
    /// </summary>
    public IValueTypeConverter<int> Int32Converter { get; } = new Int32Converter();

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

    /// <summary>
    /// The <see cref="long"/> converter.
    /// </summary>
    public IValueTypeConverter<long> LongConverter { get; } = new LongConverter();

    /// <summary>
    /// The <see cref="ulong"/> converter.
    /// </summary>
    public IValueTypeConverter<ulong> ULongConverter { get; } = new ULongConverter();
    
    /// <summary>
    /// The <see cref="uint"/> converter.
    /// </summary>
    public IValueTypeConverter<uint> UIntConverter { get; } = new UIntConverter();
    
    /// <summary>
    /// The <see cref="sbyte"/> converter.
    /// </summary>
    public IValueTypeConverter<sbyte> SbyteConverter { get; } = new SByteConverter();
    
    /// <summary>
    /// The <see cref="short"/> converter.
    /// </summary>
    public IValueTypeConverter<short> ShortConverter { get; } = new ShortConverter();
    
    /// <summary>
    /// The <see cref="byte"/> converter.
    /// </summary>
    public IValueTypeConverter<byte> ByteConverter { get; } = new ByteConverter();
    
    /// <summary>
    /// The <see cref="ushort"/> converter.
    /// </summary>
    public IValueTypeConverter<ushort> UShortConverter { get; } = new UShortConverter();
    
    /// <summary>
    /// The <see cref="DateOnly"/> converter.
    /// </summary>
    public IValueTypeConverter<DateOnly> DateOnlyConverter { get; } = new ISO8601DateOnlyConverter();

    /// <summary>
    /// The <see cref="TimeSpan"/> converter.
    /// </summary>
    public IValueTypeConverter<TimeSpan> TimeSpanConverter { get; } = new ISO8601TimeSpanConveter();

}