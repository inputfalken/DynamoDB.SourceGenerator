namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBDocument(typeof(ConstructorOnlyClass))]
[DynamoDBDocument(typeof(ObjectInitializerOnlyClass))]
[DynamoDBDocument(typeof(ObjectInitializerMixedWithConstructorClass))]
[DynamoDBDocument(typeof(ConstructorClassWithMixedName))]
public partial class InitializationTests
{

    [Fact]
    public void ConstructorOnlyClass_CanBe_Deserialized()
    {
        var @class = new ConstructorOnlyClass("Hello");
        var serializedClass = ConstructorOnlyClassDocument.Serialize(@class);
        var deserializeClass = ConstructorOnlyClassDocument.Deserialize(serializedClass);

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop1.Should().Be(@class.Prop1);
    }

    [Fact]
    public void ObjectInitializerOnlyClass_CanBe_Deserialized()
    {
        var @class = new ObjectInitializerOnlyClass {Prop2 = "Hello"};
        var serializedClass = ObjectInitializerOnlyClassDocument.Serialize(@class);
        var deserializeClass = ObjectInitializerOnlyClassDocument.Deserialize(serializedClass);

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop2.Should().Be(@class.Prop2);
    }

    [Fact]
    public void ObjectInitializerMixedWithConstructorClass_CanBe_Deserialized()
    {
        var @class = new ObjectInitializerMixedWithConstructorClass("Hello") {Prop4 = "Hello2"};
        var serializedClass = ObjectInitializerMixedWithConstructorClassDocument.Serialize(@class);
        var deserializeClass = ObjectInitializerMixedWithConstructorClassDocument.Deserialize(serializedClass);

        deserializeClass.Should().NotBeNull();
        deserializeClass.Prop3.Should().Be(@class.Prop3);
        deserializeClass.Prop4.Should().Be(@class.Prop4);
    }

    [Fact]
    public void ConstructorClassWithMixedName_UnableToFindCorrespondingDataMember_ShouldThrow()
    {
        var @class = new ConstructorClassWithMixedName("Hello");
        var serializedClass = ConstructorClassWithMixedNameDocument.Serialize(@class);
        var act = () => ConstructorClassWithMixedNameDocument.Deserialize(serializedClass);

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