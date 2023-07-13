namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public static class AssignmentExtensions
{
    public static string ToAttributeValue(this Assignment assignment)
    {
        return $"new AttributeValue {{ {assignment.Value} }}";
    }
}