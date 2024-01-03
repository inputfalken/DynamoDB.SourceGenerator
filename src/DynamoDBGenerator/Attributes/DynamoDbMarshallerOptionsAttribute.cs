using System;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Options;

namespace DynamoDBGenerator.Attributes;

/// <summary>
///     <para>
///         Contains options that will be applied for all <see cref="DynamoDBMarshallerAttribute" /> specified on a
///         <see cref="Type" />.
///     </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DynamoDbMarshallerOptionsAttribute : Attribute
{
    /// <summary>
    ///     <para>
    ///         Sets the converter <see cref="Type" /> where the source generator will look for
    ///         <see cref="IValueTypeConverter{T}" /> and <see cref="IReferenceTypeConverter{T}" />.
    ///     </para>
    ///     <para>
    ///         The default type is <see cref="AttributeValueConverters" />.
    ///     </para>
    /// </summary>
    public Type? Converters { get; set; }

    /// <summary>
    ///     <para>
    ///         Sets the conversion strategy will be used for all <see cref="Enum" />.
    ///     </para>
    ///     <para>
    ///         The default strategy is <see cref="EnumConversion.Integer" />.
    ///     </para>
    /// </summary>
    public EnumConversion EnumConversion { get; set; }
}