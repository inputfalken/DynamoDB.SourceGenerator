using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly record struct AttributeValueInstance(in string Assignment, in ITypeSymbol Type, in AttributeValueInstance.Decision How)
{
    public string Assignment { get; } = Assignment;
    public ITypeSymbol Type { get; } = Type;
    public Decision How { get; } = How;

    public enum Decision
    {
        NeedsExternalInvocation = 1,
        Inlined = 2
    }

    public override string ToString()
    {
        return $"new AttributeValue {{ {Assignment} }}";
    }
}