using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;

/// <summary>
/// 
/// </summary>
public readonly record struct AttributeValueConversionSettings(in string MPropertyMethodName)
{
    /// <summary>
    /// The generated method name that will be used for  <see cref="AttributeValue"/> property <see cref="AttributeValue.M"/>.
    /// </summary>
    public string MPropertyMethodName { get; } = MPropertyMethodName;
}