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
    public IReferenceTypeConverter<string> StringConverter { get; protected init; } = new StringConverter();

    /// <summary>
    /// The <see cref="DateTime"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTime> DateTimeConverter { get; protected init;} = new ISO8601DateTimeConverter();

    /// <summary>
    /// The <see cref="DateTimeOffsetConverter"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTimeOffset> DateTimeOffsetConverter { get; protected init;} = new ISO8601DateTimeOffsetConverter();

    /// <summary>
    /// The <see cref="bool"/> converter.
    /// </summary>
    public IValueTypeConverter<bool> BoolConverter { get; protected init;} = new BoolConverter();

    /// <summary>
    /// The <see cref="char"/> converter.
    /// </summary>
    public IValueTypeConverter<char> CharConverter { get; protected init;} = new CharConverter();

    /// <summary>
    /// The <see cref="MemoryStream"/> converter.
    /// </summary>
    public IReferenceTypeConverter<MemoryStream> MemoryStreamConverter { get; protected init;} = new MemoryStreamConverter();

    /// <summary>
    /// The <see cref="int"/> converter.
    /// </summary>
    public IValueTypeConverter<int> Int32Converter { get; protected init; } = new Int32Converter();

    /// <summary>
    /// The <see cref="decimal"/> converter.
    /// </summary>
    public IValueTypeConverter<decimal> DecimalConverter { get; protected init;} = new DecimalConverter();

    /// <summary>
    /// The <see cref="double"/> converter.
    /// </summary>
    public IValueTypeConverter<double> DoubleConverter { get; protected init;} = new DoubleConverter();

    /// <summary>
    /// The <see cref="float"/> converter.
    /// </summary>
    public IValueTypeConverter<float> FloatConverter { get; protected init;} = new FloatConverter();

    /// <summary>
    /// The <see cref="long"/> converter.
    /// </summary>
    public IValueTypeConverter<long> LongConverter { get; protected init;} = new LongConverter();

    /// <summary>
    /// The <see cref="ulong"/> converter.
    /// </summary>
    public IValueTypeConverter<ulong> ULongConverter { get; protected init;} = new ULongConverter();
    
    /// <summary>
    /// The <see cref="uint"/> converter.
    /// </summary>
    public IValueTypeConverter<uint> UIntConverter { get; protected init;} = new UIntConverter();
    
    /// <summary>
    /// The <see cref="sbyte"/> converter.
    /// </summary>
    public IValueTypeConverter<sbyte> SbyteConverter { get; protected init;} = new SByteConverter();
    
    /// <summary>
    /// The <see cref="short"/> converter.
    /// </summary>
    public IValueTypeConverter<short> ShortConverter { get; protected init;} = new ShortConverter();
    
    /// <summary>
    /// The <see cref="byte"/> converter.
    /// </summary>
    public IValueTypeConverter<byte> ByteConverter { get; protected init;} = new ByteConverter();
    
    /// <summary>
    /// The <see cref="ushort"/> converter.
    /// </summary>
    public IValueTypeConverter<ushort> UShortConverter { get; protected init;} = new UShortConverter();
    
    /// <summary>
    /// The <see cref="DateOnly"/> converter.
    /// </summary>
    public IValueTypeConverter<DateOnly> DateOnlyConverter { get; protected init; } = new ISO8601DateOnlyConverter();

    /// <summary>
    /// The <see cref="TimeSpan"/> converter.
    /// </summary>
    public IValueTypeConverter<TimeSpan> TimeSpanConverter { get; protected init;} = new ISO8601TimeSpanConveter();
    
    /// <summary>
    /// The <see cref="Guid"/> converter.
    /// </summary>
    public IValueTypeConverter<Guid> GuidConverter { get; protected init;} = new GuidConverter();

}