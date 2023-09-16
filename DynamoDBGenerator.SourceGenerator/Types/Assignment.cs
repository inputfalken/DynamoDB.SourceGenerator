using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Assignment(in string Value, in ITypeSymbol Type, in bool HasExternalDependency)
{
    public string Value { get; } = Value;

    /// <summary>
    ///     The type the assignment was based on.
    /// </summary>
    public ITypeSymbol Type { get; } = Type;

    /// <summary>
    ///     The assignment decision.
    /// </summary>
    // TODO convert this into KnownType? where a nullable KnownType will have the intent that it's unknown and requires external help.
    public bool HasExternalDependency { get; } = HasExternalDependency;
}