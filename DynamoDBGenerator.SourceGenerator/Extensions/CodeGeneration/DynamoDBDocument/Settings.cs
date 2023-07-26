using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.DynamoDBDocument;

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

public record MethodConfiguration(in string Name)
{
    /// <summary>
    ///     The name method.
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// Determines the access modifier.
    /// </summary>
    public Accessibility AccessModifier { get; set; } = Accessibility.Public;
}