namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly record struct SourceGeneratedCode(in string Code, string MethodName)
{
    public string Code { get; } = Code;
    public string MethodName { get; } = MethodName;

}