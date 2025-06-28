using System;
using System.Collections.Generic;
using System.IO;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Converters.Internal;
using DynamoDBGenerator.Internal;

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
    public IReferenceTypeConverter<string> StringConverter { get; protected init; } = Singleton.Static<StringConverter>();

    /// <summary>
    /// The <see cref="DateTime"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTime> DateTimeConverter { get; protected init;} = Singleton.Static<ISO8601DateTimeConverter>();

    /// <summary>
    /// The <see cref="DateTimeOffsetConverter"/> converter.
    /// </summary>
    public IValueTypeConverter<DateTimeOffset> DateTimeOffsetConverter { get; protected init;} = Singleton.Static<ISO8601DateTimeOffsetConverter>();

    /// <summary>
    /// The <see cref="bool"/> converter.
    /// </summary>
    public IValueTypeConverter<bool> BoolConverter { get; protected init;} = Singleton.Static<BoolConverter>();

    /// <summary>
    /// The <see cref="char"/> converter.
    /// </summary>
    public IValueTypeConverter<char> CharConverter { get; protected init;} = Singleton.Static<CharConverter>();

    /// <summary>
    /// The <see cref="MemoryStream"/> converter.
    /// </summary>
    public IReferenceTypeConverter<MemoryStream> MemoryStreamConverter { get; protected init;} = Singleton.Static<MemoryStreamConverter>();

    /// <summary>
    /// The <see cref="int"/> converter.
    /// </summary>
    public IValueTypeConverter<int> Int32Converter { get; protected init; } = Singleton.Static<Int32Converter>();

    /// <summary>
    /// The <see cref="decimal"/> converter.
    /// </summary>
    public IValueTypeConverter<decimal> DecimalConverter { get; protected init;} = Singleton.Static<DecimalConverter>();

    /// <summary>
    /// The <see cref="double"/> converter.
    /// </summary>
    public IValueTypeConverter<double> DoubleConverter { get; protected init;} = Singleton.Static<DoubleConverter>();

    /// <summary>
    /// The <see cref="float"/> converter.
    /// </summary>
    public IValueTypeConverter<float> FloatConverter { get; protected init;} = Singleton.Static<FloatConverter>();

    /// <summary>
    /// The <see cref="long"/> converter.
    /// </summary>
    public IValueTypeConverter<long> LongConverter { get; protected init;} = Singleton.Static<LongConverter>();

    /// <summary>
    /// The <see cref="ulong"/> converter.
    /// </summary>
    public IValueTypeConverter<ulong> ULongConverter { get; protected init;} = Singleton.Static<ULongConverter>();
    
    /// <summary>
    /// The <see cref="uint"/> converter.
    /// </summary>
    public IValueTypeConverter<uint> UIntConverter { get; protected init;} = Singleton.Static<UIntConverter>();

    /// <summary>
    /// The <see cref="sbyte"/> converter.
    /// </summary>
    public IValueTypeConverter<sbyte> SbyteConverter { get; protected init; } = Singleton.Static<SByteConverter>();
    
    /// <summary>
    /// The <see cref="short"/> converter.
    /// </summary>
    public IValueTypeConverter<short> ShortConverter { get; protected init;} = Singleton.Static<ShortConverter>();
    
    /// <summary>
    /// The <see cref="byte"/> converter.
    /// </summary>
    public IValueTypeConverter<byte> ByteConverter { get; protected init;} = Singleton.Static<ByteConverter>();
    
    /// <summary>
    /// The <see cref="ushort"/> converter.
    /// </summary>
    public IValueTypeConverter<ushort> UShortConverter { get; protected init;} = Singleton.Static<UShortConverter>();
    
    /// <summary>
    /// The <see cref="DateOnly"/> converter.
    /// </summary>
    public IValueTypeConverter<DateOnly> DateOnlyConverter { get; protected init; } = Singleton.Static<ISO8601DateOnlyConverter>();

    /// <summary>
    /// The <see cref="TimeOnly"/> converter.
    /// </summary>
    public IValueTypeConverter<TimeOnly> TimeOnlyConverter { get; protected init; } = Singleton.Static<ISO8601TimeOnlyConverter>();

    /// <summary>
    /// The <see cref="TimeSpan"/> converter.
    /// </summary>
    public IValueTypeConverter<TimeSpan> TimeSpanConverter { get; protected init; } = Singleton.Static<ISO8601TimeSpanConverter>();

    /// <summary>
    /// The <see cref="Guid"/> converter.
    /// </summary>
    public IValueTypeConverter<Guid> GuidConverter { get; protected init; } = Singleton.Static<GuidConverter>();
    
    /// <summary>
    /// The <see cref="ISet{T}"/> converter for <see cref="string"/> sets.
    /// </summary>
    public IReferenceTypeConverter<ISet<string>> NoneNullableStringISetConverter { get; protected init; } = Singleton.Static<NoneNullableStringSetConverter>();

    /// <summary>
    /// The <see cref="IReadOnlySet{T}"/> converter for <see cref="string"/> sets.
    /// </summary>
    public IReferenceTypeConverter<IReadOnlySet<string>> NoneNullableStringIReadOnlySetConverter { get; protected init; } = Singleton.Static<NoneNullableStringSetConverter>();

    /// <summary>
    /// The <see cref="HashSet{T}"/> converter for <see cref="string"/> sets.
    /// </summary>
    public IReferenceTypeConverter<HashSet<string>> NoneNullableStringHashSetConverter { get; protected init; } = Singleton.Static<NoneNullableStringSetConverter>();

    /// <summary>
    /// The <see cref="SortedSet{T}"/> converter for <see cref="string"/> sets.
    /// </summary>
    public IReferenceTypeConverter<SortedSet<string>> NoneNullableStringSortedSetConverter { get; protected init; } = Singleton.Static<NoneNullableStringSetConverter>();
}