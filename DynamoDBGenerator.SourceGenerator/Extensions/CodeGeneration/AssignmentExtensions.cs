namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public static class AssignmentExtensions
{
    public static string ToAttributeValue(this Assignment assignment, bool isImplicit = false)
    {
        return isImplicit ? $"new {{ {assignment.Value} }}" : $"new AttributeValue {{ {assignment.Value} }}";
    }
}