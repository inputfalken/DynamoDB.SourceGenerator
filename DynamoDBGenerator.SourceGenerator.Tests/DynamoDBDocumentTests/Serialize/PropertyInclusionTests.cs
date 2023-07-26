namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBDocument(typeof(EmptyClass))]
[DynamoDBDocument(typeof(ClassWithIgnoredProperty))]
[DynamoDBDocument(typeof(ClassWithOneProperty))]
[DynamoDBDocument(typeof(ClassWithOneDDBProperty))]
public partial class PropertyInclusionTests
{

    [Fact]
    public void Serialize_ClassWithDDbProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneDDBProperty {Id = "I should be included"};

        ClassWithOneDDBPropertyDocument
            .Serialize(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneDDBProperty.Id));
    }

    [Fact]
    public void Serialize_ClassWithIgnoredProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithIgnoredProperty
            {Id = "I should not be exists in attribute values"};

        ClassWithIgnoredPropertyDocument
            .Serialize(@class)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Serialize_ClassWithProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneProperty {Id = "I should be included"};

        ClassWithOnePropertyDocument
            .Serialize(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneProperty.Id));
    }
    [Fact]
    public void Serialize_EmptyClass_AttributeValueIsEmpty()
    {
        var @class = new EmptyClass();

        EmptyClassDocument
            .Serialize(@class)
            .Should()
            .BeEmpty();
    }
}

public class ClassWithOneDDBProperty
{
    [DynamoDBProperty]
    public string? Id { get; set; }
}

public class ClassWithOneProperty
{
    public string? Id { get; set; }
}

public class EmptyClass
{
}

public class ClassWithIgnoredProperty
{
    [DynamoDBIgnore]
    public string? Id { get; set; }
}
