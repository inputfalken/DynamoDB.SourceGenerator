namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class FieldInclusionTests
{
    [Fact]
    public void BuildAttributeValues_EmptyClass_AttributeValueIsEmpty()
    {
        var @class = new EmptyClass();

        var result = @class.BuildAttributeValues();

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_ClassWithIgnoredField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithIgnoredField() {Id = "I should not be exists in attribute values"};

        var result = @class.BuildAttributeValues();

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_ClassWithField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneField {Id = "I should be included"};

        var result = @class.BuildAttributeValues();

        result.Should().ContainKey(nameof(ClassWithOneField.Id));
    }

    [Fact]
    public void BuildAttributeValues_ClassWithDDbField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneDDBField {Id = "I should be included"};

        var result = @class.BuildAttributeValues();

        result.Should().ContainKey(nameof(ClassWithOneDDBField.Id));
    }
}

[AttributeValueGenerator]
public partial class ClassWithOneDDBField
{
    [DynamoDBProperty] public string Id;
}

[AttributeValueGenerator]
public partial class ClassWithOneField
{
    public string Id;
}

[AttributeValueGenerator]
public partial class ClassWithIgnoredField
{
    [DynamoDBIgnore] public string Id;
}