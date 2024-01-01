using System;

namespace DynamoDBGenerator.Options;

/// <summary>
/// The strategy to persist and read <see cref="Enum"/>.
/// </summary>
public enum EnumConversionStrategy
{
    // TODO convert this into a bitflag so you can do Integer | String | StringCI and specify the order of operations if you would like to support multiple.
    /// <summary>
    /// Use the associated constant <see cref="int"/> value.
    /// </summary>
    Integer = 0,

    /// <summary>
    /// Use the name representation with case sensitivity.
    /// </summary>
    String = 1,

    /// <summary>
    /// Use the name representation with case insensitivity.
    /// </summary>
    StringCI = 2
}