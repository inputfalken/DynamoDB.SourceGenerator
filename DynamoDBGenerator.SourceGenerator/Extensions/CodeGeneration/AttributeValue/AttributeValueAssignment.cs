using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

public readonly record struct AttributeValueAssignment(
    in string Assignment,
    in ITypeSymbol Type,
    in AttributeValueAssignment.Decision AssignedBy
)
{
    public enum Decision
    {
        ExternalMethod = 1,
        Inline = 2
    }

    /// <summary>
    ///     The C# assignment code.
    /// </summary>
    public string Assignment { get; } = Assignment;

    /// <summary>
    ///     The type the assignment was based on.
    /// </summary>
    public ITypeSymbol Type { get; } = Type;

    /// <summary>
    ///     The assignment decision.
    /// </summary>
    public Decision AssignedBy { get; } = AssignedBy;

    public override string ToString()
    {
        return $"new AttributeValue {{ {Assignment} }}";
    }
}