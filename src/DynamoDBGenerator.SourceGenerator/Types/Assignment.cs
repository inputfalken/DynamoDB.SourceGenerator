namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Assignment(string Value, TypeIdentifier KnownType)
{
    public string Value { get; } = Value;


    /// <summary>
    ///     Determines whether the type was handled.
    /// </summary>
    public TypeIdentifier KnownType { get; } = KnownType;
}