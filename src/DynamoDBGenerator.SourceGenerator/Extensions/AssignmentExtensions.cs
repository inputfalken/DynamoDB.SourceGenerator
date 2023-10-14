using DynamoDBGenerator.SourceGenerator.Types;
namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class AssignmentExtensions
{
    public static string ToAttributeValue(this Assignment assignment)
    {
        return $"new AttributeValue {{ {assignment.Value} }}";
    }
}