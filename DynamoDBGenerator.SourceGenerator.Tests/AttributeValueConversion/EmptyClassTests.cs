namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class EmptyClassTests
{
    [Fact]
    public void BuildAttributeValues_EmptyClass_AttributeValueIsEmpty()
    {
        var @class = new EmptyClass();

        var result = @class.BuildAttributeValues();

        result.Should().BeEmpty();
    }
    
}

[AttributeValueGenerator]
public partial class EmptyClass
{
    
}