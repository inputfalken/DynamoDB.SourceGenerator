using System;

namespace DynamoDBGenerator.Attributes;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DynamoDbMarshallerOptionsAttribute : Attribute
{
    /// <summary>
    /// A converters type that defaults to <see cref="Converters.AttributeValueConverters"/>.
    /// You can override this type with your own in order to provide a custom converters by providing a type that inherits from the default.
    /// </summary>
    public Type Converters { get; set; }
}