using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly struct AttributeValueInstance
{
    public AttributeValueInstance(in string assignment, in ITypeSymbol type, in Decision how)
    {
        Assignment = assignment;
        Type = type;
        How = how;
    }

    public string Assignment { get; }
    public ITypeSymbol Type { get; }
    public Decision How { get; }

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