using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public readonly record struct Assignment
{
    private readonly string _assignment;

    public Assignment(in string assignment,
        in ITypeSymbol Type,
        in Decision AssignedBy)
    {
        _assignment = assignment;
        this.Type = Type;
        this.AssignedBy = AssignedBy;
    }

    public enum Decision
    {
        ExternalMethod = 1,
        Inline = 2
    }


    /// <summary>
    ///     The type the assignment was based on.
    /// </summary>
    public ITypeSymbol Type { get; }

    /// <summary>
    ///     The assignment decision.
    /// </summary>
    public Decision AssignedBy { get; }

    public override string ToString()
    {
        return $"new AttributeValue {{ {_assignment} }}";
    }
}