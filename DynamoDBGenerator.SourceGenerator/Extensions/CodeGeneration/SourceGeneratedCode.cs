namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly record struct SourceGeneratedCode(in string Code, in string ClassName,
    string MethodName)
{
    public string Code { get; } = Code;
    public string ClassName { get; } = ClassName;
    public string MethodName { get; } = MethodName;

    public override string ToString()
    {
        return $"{ClassName}.{MethodName}";
    }
}