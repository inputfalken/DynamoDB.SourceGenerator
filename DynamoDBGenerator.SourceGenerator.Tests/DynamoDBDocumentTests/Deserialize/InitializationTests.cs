using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBMarshaller(typeof(ConstructorOnlyClass))]
[DynamoDBMarshaller(typeof(ObjectInitializerOnlyClass))]
[DynamoDBMarshaller(typeof(ObjectInitializerMixedWithConstructorClass))]
[DynamoDBMarshaller(typeof(ConstructorClassWithMixedName))]
public partial class InitializationTests
{

    [Fact]
    public void ConstructorOnlyClass_CanBe_Deserialized()
    {
        var deserializeClass = ConstructorOnlyClassMarshaller.Deserialize(
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
    public void ObjectInitializerOnlyClass_CanBe_Deserialized()
    {
        var @class = new ObjectInitializerOnlyClass {Prop2 = "Hello"};
        var serializedClass = ObjectInitializerOnlyClassMarshaller.Serialize(@class);
        var deserializeClass = ObjectInitializerOnlyClassMarshaller.Deserialize(serializedClass);

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop2.Should().Be(@class.Prop2);
    }

    [Fact]
    public void ObjectInitializerMixedWithConstructorClass_CanBe_Deserialized()
    {
        var deserializeClass = ObjectInitializerMixedWithConstructorClassMarshaller.Deserialize(new Dictionary<string, AttributeValue>
        {
            {"Prop3", new AttributeValue {S = "Hello"}},
            {"Prop4", new AttributeValue {S = "Hello2"}}

        });

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop3.Should().Be("Hello");
        deserializeClass.Prop4.Should().Be("Hello2");
    }

    [Fact]
    public void ConstructorClassWithMixedName_UnableToFindCorrespondingDataMember_ShouldThrow()
    {
        var act = () => ConstructorClassWithMixedNameMarshaller.Deserialize(new Dictionary<string, AttributeValue>
        {
            {"SomethingElse", new AttributeValue {S = "Hello"}}
        });

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Unable to determine the corresponding data member for constructor argument 'something' in 'ConstructorClassWithMixedName'; make sure the names are consistent.");
    }
}

public class ConstructorOnlyClass
{
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
    public ObjectInitializerMixedWithConstructorClass(string prop3)
    {
        Prop3 = prop3;
    }

    public string Prop3 { get; }
    public string? Prop4 { get; set; }
}

public class ConstructorClassWithMixedName
{

    public ConstructorClassWithMixedName(string something)
    {

        SomethingElse = something;
    }
    public string SomethingElse { get; }
}