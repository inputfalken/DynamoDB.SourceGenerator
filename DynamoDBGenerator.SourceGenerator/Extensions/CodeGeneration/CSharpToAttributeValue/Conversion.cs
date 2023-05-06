using DynamoDBGenerator.SourceGenerator.Types;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public readonly record struct Conversion(
    in string Code,
    in string MethodName,
    in IEnumerable<Conversion<DynamoDbDataMember, Assignment>> Conversions)
{
    public string MethodName { get; } = MethodName;

    /// <summary>
    ///     The C# code for the <see cref="AttributeValue" /> conversion.
    /// </summary>
    public string Code { get; } = Code;

    /// <summary>
    ///     The conversions that occur within the method.
    /// </summary>
    public IEnumerable<Conversion<DynamoDbDataMember, Assignment>> Conversions { get; } = Conversions;
}