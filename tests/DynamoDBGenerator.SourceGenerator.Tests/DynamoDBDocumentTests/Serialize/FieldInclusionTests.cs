using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(EntityType = typeof(EmptyClass))]
[DynamoDBMarshaller(EntityType = typeof(ClassWithIgnoredField))]
[DynamoDBMarshaller(EntityType = typeof(ClassWithOneField))]
[DynamoDBMarshaller(EntityType = typeof(ClassWithOneDDBField))]
public partial class FieldInclusionTests
{
    [Fact]
    public void Serialize_ClassWithDDbField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneDDBField { Id = "I should be included" };

        ClassWithOneDDBFieldMarshaller
            .Marshall(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneDDBField.Id));
    }

    [Fact]
    public void Serialize_ClassWithField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithOneField { Id = "I should be included" };

        ClassWithOneFieldMarshaller
            .Marshall(@class)
            .Should()
            .ContainKey(nameof(ClassWithOneField.Id));
    }

    [Fact]
    public void Serialize_ClassWithIgnoredField_AttributeValueIsEmpty()
    {
        var @class = new ClassWithIgnoredField
            { Id = "I should not be exists in attribute values" };

        ClassWithIgnoredFieldMarshaller
            .Marshall(@class)
            .Should()
            .BeEmpty();
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