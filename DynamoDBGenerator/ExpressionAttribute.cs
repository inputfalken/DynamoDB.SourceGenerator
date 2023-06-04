namespace DynamoDBGenerator;

public record ExpressionAttribute(string Name, string Value)
{
    public string Name { get; } = Name;
    public string Value { get; } = Value;
}