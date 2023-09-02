namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(typeof(EmptyClass))]
[DynamoDBMarshaller(typeof(ClassWithIgnoredProperty))]
[DynamoDBMarshaller(typeof(ClassWithOneProperty))]
[DynamoDBMarshaller(typeof(ClassWithOneDDBProperty))]
public partial class PropertyInclusionTests
{

    [Fact]
    public void Serialize_ClassWithDDbProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneDDBProperty {Id = "I should be included"};

        ClassWithOneDDBPropertyMarshaller
            .Marshall(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneDDBProperty.Id));
    }

    [Fact]
    public void Serialize_ClassWithIgnoredProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithIgnoredProperty
            {Id = "I should not be exists in attribute values"};

        ClassWithIgnoredPropertyMarshaller
            .Marshall(@class)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Serialize_ClassWithProperty_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneProperty {Id = "I should be included"};

        ClassWithOnePropertyMarshaller
            .Marshall(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneProperty.Id));
    }
    [Fact]
    public void Serialize_EmptyClass_AttributeValueIsEmpty()
    {
        var @class = new EmptyClass();

        EmptyClassMarshaller
            .Marshall(@class)
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
