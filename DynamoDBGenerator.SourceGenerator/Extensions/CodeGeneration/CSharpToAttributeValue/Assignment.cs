using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public readonly record struct SourceGeneratedAttributeValueFactory(in string Code, in string ClassName, string MethodName)
{
    public string Code { get; } = Code;
    public string ClassName { get; } = ClassName;
    public string MethodName { get; } = MethodName;

    public override string ToString()
    {
        return $"{ClassName}.{MethodName}";
    }
}

public readonly record struct Assignment
{
    public enum Decision
    {
        ExternalMethod = 1,
        Inline = 2
    }

    private readonly string _assignment;

    public Assignment(in string assignment,
        in ITypeSymbol Type,
        in Decision AssignedBy)
    {
        _assignment = assignment;
        this.Type = Type;
        this.AssignedBy = AssignedBy;
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