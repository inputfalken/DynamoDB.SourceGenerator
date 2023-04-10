namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class ReferenceTypeTests
{
    [Fact]
    public void BuildAttributeValues_ReferenceType_DefaultValueIsNotIncluded()
    {
        var @class = new ReferenceTypeClass();

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }
}

[AttributeValueGenerator]
public partial class ReferenceTypeClass
{
    [DynamoDBProperty]
    public string ReferenceType { get; set; }
}