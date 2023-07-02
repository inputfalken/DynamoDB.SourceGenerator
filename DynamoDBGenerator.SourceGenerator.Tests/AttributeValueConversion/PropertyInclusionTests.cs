namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class PropertyInclusionTests
{
    [Fact]
    public void BuildAttributeValues_EmptyClass_AttributeValueIsEmpty()
    {
        var @class = new EmptyClass();

        var result = @class.BuildAttributeValues();

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_ClassWithIgnoredProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithIgnoredProperty() {Id = "I should not be exists in attribute values"};

        var result = @class.BuildAttributeValues();

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_ClassWithProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneProperty {Id = "I should be included"};

        var result = @class.BuildAttributeValues();

        result.Should().ContainKey(nameof(ClassWithOneProperty.Id));
    }

    [Fact]
    public void BuildAttributeValues_ClassWithDDbProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneDDBProperty {Id = "I should be included"};

        var result = @class.BuildAttributeValues();

        result.Should().ContainKey(nameof(ClassWithOneDDBProperty.Id));
    }
}

[DynamoDbDocument]
public partial class ClassWithOneDDBProperty
{
    [DynamoDBProperty]
    public string? Id { get; set; }
}

[DynamoDbDocument]
public partial class ClassWithOneProperty
{
    public string? Id { get; set; }
}

[DynamoDbDocument]
public partial class EmptyClass
{
}

[DynamoDbDocument]
public partial class ClassWithIgnoredProperty
{
    [DynamoDBIgnore]
    public string? Id { get; set; }
}