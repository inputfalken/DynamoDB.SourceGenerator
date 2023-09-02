namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshallert(typeof(EmptyClass))]
[DynamoDBMarshallert(typeof(ClassWithIgnoredField))]
[DynamoDBMarshallert(typeof(ClassWithOneField))]
[DynamoDBMarshallert(typeof(ClassWithOneDDBField))]
public partial class FieldInclusionTests
{

    [Fact]
    public void Serialize_ClassWithDDbField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneDDBField {Id = "I should be included"};

        ClassWithOneDDBFieldDocument
            .Serialize(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneDDBField.Id));
    }

    [Fact]
    public void Serialize_ClassWithField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneField {Id = "I should be included"};

        ClassWithOneFieldDocument
            .Serialize(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneField.Id));
    }

    [Fact]
    public void Serialize_ClassWithIgnoredField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithIgnoredField
            {Id = "I should not be exists in attribute values"};

        ClassWithIgnoredFieldDocument
            .Serialize(@class)
            .Should()
            .BeEmpty();
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

public class ClassWithOneDDBField
{
    [DynamoDBProperty] public string? Id;
}

public class ClassWithOneField
{
    public string? Id;
}

public class ClassWithIgnoredField
{
    [DynamoDBIgnore] public string? Id;
}
