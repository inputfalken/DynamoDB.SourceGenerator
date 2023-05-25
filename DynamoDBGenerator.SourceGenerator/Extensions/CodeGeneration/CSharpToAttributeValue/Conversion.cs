namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public readonly record struct Conversion(
    in string Code,
    in IEnumerable<Assignment> Conversions)
{
    /// <summary>
    ///     The C# code for the <see cref="Amazon.DynamoDBv2.Model.AttributeValue" /> conversion.
    /// </summary>
    public string Code { get; } = Code;

    /// <summary>
    ///     The conversions that occur within the method.
    /// </summary>
    public IEnumerable<Assignment> Conversions { get; } = Conversions;
}