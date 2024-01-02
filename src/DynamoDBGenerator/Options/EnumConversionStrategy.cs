using System;

namespace DynamoDBGenerator.Options;

/// <summary>
/// The strategy to persist and read <see cref="Enum"/>.
/// </summary>
public enum EnumConversionStrategy
{
    /// <summary>
    /// Use the associated constant <see cref="int"/> value.
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Use the name representation with case sensitivity.
    /// </summary>
    String = 2,

    /// <summary>
    /// Use the name representation with case insensitivity.
    /// </summary>
    StringCI = 4
}