using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

/// <summary>
/// </summary>
public readonly record struct Settings(in string MPropertyMethodName, in string ConsumerMethodName)
{
    /// <summary>
    ///     The generated method name that will be used for  <see cref="AttributeValue" /> property
    ///     <see cref="AttributeValue.M" />.
    /// </summary>
    public string MPropertyMethodName { get; } = MPropertyMethodName;

    /// <summary>
    /// The name of the method available for public usage by the consumer.
    /// </summary>
    public string ConsumerMethodName { get; } = ConsumerMethodName;
}