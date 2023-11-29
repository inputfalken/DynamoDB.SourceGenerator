namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Assignment(string Value, TypeIdentifier TypeIdentifier)
{
    /// <summary>
    /// The assignment value
    /// </summary>
    public string Value { get; } = Value;

    /// <summary>
    ///     Determines whether the type was handled.
    /// </summary>
    public TypeIdentifier TypeIdentifier { get; } = TypeIdentifier;
}