namespace DynamoDBGenerator.SourceGenerator.Enums;

public enum KeyStrategy
{
    /// <summary>
    /// Include keys and everything else.
    /// </summary>
    Include = 1,

    /// <summary>
    /// Ignore DynamoDB key properties.
    /// </summary>
    Ignore = 2,

    /// <summary>
    /// Only include DynamoDB key properties.
    /// </summary>
    Only = 3,
}