using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBMarshaller(typeof(ConstructorOnlyClass))]
[DynamoDBMarshaller(typeof(ObjectInitializerOnlyClass))]
[DynamoDBMarshaller(typeof(ObjectInitializerMixedWithConstructorClass))]
[DynamoDBMarshaller(typeof(ConstructorClassWithMixedName))]
[DynamoDBMarshaller(typeof(InlinedRecord))]
[DynamoDBMarshaller(typeof(ExplicitConstructorRecord))]
[DynamoDBMarshaller(typeof(InlineRecordWithNestedRecord))]
public partial class InitializationTests
{

    [Fact]
    public void ConstructorOnlyClass_FindCorrespondingDataMember_ShouldSucceed()
    {
        var deserializeClass = ConstructorOnlyClassMarshaller.Unmarshall(
            new Dictionary<string, AttributeValue>
            {
                {
                    "Prop1", new AttributeValue {S = "Hello"}
                }
            }
        );

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop1.Should().Be(@"Hello");
    }

    [Fact]
    public void ObjectInitializerOnlyClass_FindCorrespondingDataMember_ShouldSucceed()
    {
        var @class = new ObjectInitializerOnlyClass {Prop2 = "Hello"};
        var serializedClass = ObjectInitializerOnlyClassMarshaller.Marshall(@class);
        var deserializeClass = ObjectInitializerOnlyClassMarshaller.Unmarshall(serializedClass);

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop2.Should().Be(@class.Prop2);
    }

    [Fact]
    public void ObjectInitializerMixedWithConstructorClass_FindCorrespondingDataMember_ShouldSucceed()
    {
        var deserializeClass = ObjectInitializerMixedWithConstructorClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            {"Prop3", new AttributeValue {S = "Hello"}},
            {"Prop4", new AttributeValue {S = "Hello2"}}

        });

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop3.Should().Be("Hello");
        deserializeClass.Prop4.Should().Be("Hello2");
    }

    [Fact]
    public void ConstructorClassWithMixedName_FindCorrespondingDataMember_ShouldSucceed()
    {
        var result = ConstructorClassWithMixedNameMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            {"SomethingElse", new AttributeValue {S = "Hello"}}
        });

        result.SomethingElse.Should().Be("Hello");
    }
    [Fact]
    public void InlineRecord_FindingCorrespondingDataMembers_ShouldSucceed()
    {
        var inlinedRecord = InlinedRecordMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            {"FirstProperty", new AttributeValue {S = "Hello"}},
            {"SecondProperty", new AttributeValue {S = "World"}}
        });

        inlinedRecord.FirstProperty.Should().Be("Hello");
        inlinedRecord.SecondProperty.Should().Be("World");
    }
    [Fact]
    public void ExplicitConstructorRecord_FindingCorrespondingDataMembers_ShouldSucceed()
    {
        var inlinedRecord = ExplicitConstructorRecordMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            {"FirstProperty", new AttributeValue {S = "Hello"}},
            {"SecondProperty", new AttributeValue {S = "World"}}
        });

        inlinedRecord.FirstProperty.Should().Be("Hello");
        inlinedRecord.SecondProperty.Should().Be("World");
    }
}

public record InlineRecordWithNestedRecord(string One, InlineRecordWithNestedRecord.InlinedNestedRecord Test)
{
    public record InlinedNestedRecord(string Two);
}
public record InlinedRecord(string FirstProperty, string SecondProperty);

public record ExplicitConstructorRecord
{

    [DynamoDBMarshallerConstructor]
    public ExplicitConstructorRecord(string first, string second)
    {
        FirstProperty = first;
        SecondProperty = second;
    }
    public string FirstProperty { get; init; }
    public string SecondProperty { get; init; }
}

public class ConstructorOnlyClass
{
    [DynamoDBMarshallerConstructor]
    public ConstructorOnlyClass(string prop1)
    {
        Prop1 = prop1;
    }
    public string Prop1 { get; }
}

public class ObjectInitializerOnlyClass
{
    public string? Prop2 { get; init; }
}

public class ObjectInitializerMixedWithConstructorClass
{
    [DynamoDBMarshallerConstructor]
    public ObjectInitializerMixedWithConstructorClass(string prop3)
    {
        Prop3 = prop3;
    }

    public string Prop3 { get; }
    public string? Prop4 { get; set; }
}

public class ConstructorClassWithMixedName
{
    [DynamoDBMarshallerConstructor]
    public ConstructorClassWithMixedName(string something)
    {

        SomethingElse = something;
    }
    public string SomethingElse { get; }
}