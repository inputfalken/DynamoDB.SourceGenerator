namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class MethodRenameTests
{
    public const string MethodName = "MyAttributeConversion";

    [Fact]
    public void MyAttributeConversion_Name_IsOverriden()
    {
        typeof(OverridenMethodName).GetMethod(MethodName).Should().NotBeNull();
    }

    [Fact]
    public void BuildAttributeValues_Name_IsOverriden()
    {
        typeof(DefaultName).GetMethod(AttributeValueGeneratorAttribute.DefaultMethodName).Should().NotBeNull();
    }
}

[AttributeValueGenerator]
public partial class DefaultName
{
    public string FooBar { get; set; }
}

[AttributeValueGenerator(methodName: MethodRenameTests.MethodName)]
public partial class OverridenMethodName
{
    public string FooBar { get; set; }
}