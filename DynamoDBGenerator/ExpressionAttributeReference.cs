using System;

namespace DynamoDBGenerator;

public readonly record struct AttributeReference
{
    private readonly Lazy<string> _name;
    private readonly Lazy<string> _value;

    public AttributeReference(in Lazy<string> name, in Lazy<string> value)
    {
        _name = name;
        _value = value;
    }

    /// <summary>
    /// Dynamodb column reference.
    /// </summary>
    public string Name => _name.Value;

    /// <summary>
    /// update value provided in execution.
    /// </summary>
    public string Value => _value.Value;
}