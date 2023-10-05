using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Assignment(in string Value, in ITypeSymbol Type, in KnownType? KnownType)
{
    public string Value { get; } = Value;

    /// <summary>
    ///     The type the assignment was based on.
    /// </summary>
    public ITypeSymbol Type { get; } = Type;

    /// <summary>
    ///     Determines whether the type was handled.
    /// </summary>
    public KnownType? KnownType { get; } = KnownType;
}