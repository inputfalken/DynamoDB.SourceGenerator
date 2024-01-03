using System;

namespace DynamoDBGenerator.Options;

/// <summary>
///     The strategy to persist and read <see cref="Enum" />.
/// </summary>
public enum EnumConversion
{
    /// <summary>
    ///     <para>
    ///         Use the underlying <see cref="int" /> value.
    ///     </para>
    /// </summary>
    Integer = 1,

    /// <summary>
    ///     <para>
    ///         Use the name representation by calling <see cref="Enum.ToString()" />.
    ///     </para>
    ///     <remarks>
    ///         This will be case sensitive when parsing the <see cref="Enum" />.
    ///     </remarks>
    /// </summary>
    Name = 2,

    /// <summary>
    ///     <para>
    ///         Use the name representation by calling <see cref="Enum.ToString()" />.
    ///     </para>
    ///     <remarks>
    ///         This will be case insensitive when parsing the <see cref="Enum" />.
    ///     </remarks>
    /// </summary>
    CaseInsensitiveName = 3,

    /// <summary>
    ///     <para>
    ///         Use the name representation by calling <see cref="Enum.ToString()" /> followed by
    ///         <see cref="string.ToLowerInvariant()" />.
    ///     </para>
    ///     <remarks>
    ///         This will be case insensitive when parsing the <see cref="Enum" />.
    ///     </remarks>
    /// </summary>
    LowerCaseName = 4,

    /// <summary>
    ///     <para>
    ///         Use the name representation by calling <see cref="Enum.ToString()" /> followed by
    ///         <see cref="string.ToUpperInvariant()" />.
    ///     </para>
    ///     <remarks>
    ///         This will be case insensitive when parsing the <see cref="Enum" />.
    ///     </remarks>
    /// </summary>
    UpperCaseName = 5
}