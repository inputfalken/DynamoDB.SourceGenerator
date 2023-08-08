using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.CustomObjects;

[DynamoDBDocument(typeof(ParentClass))]
public partial class ChildClassTests
{
    [Fact]
    public void Deserialize_AllFieldsSet_AllIncluded()
    {
        var result = ParentClassDocument.Deserialize(new Dictionary<string, AttributeValue>
        {
            {nameof(ParentClass.Id), new AttributeValue {S = "I am the root"}},
            {
                nameof(ParentClass.CustomClass), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {

                        {nameof(ParentClass.CustomClass.PropertyId), new AttributeValue {S = "I am the property"}},
                        {
                            nameof(ParentClass.CustomClass.GrandChild), new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {nameof(ParentClass.ChildClass.GrandChildClass.GrandChildId), new AttributeValue {S = "I am the grandchild id"}}
                                }
                            }
                        }
                    }
                }
            }
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().NotBeNull();
        result.CustomClass!.PropertyId.Should().Be("I am the property");
        result.CustomClass.GrandChild.Should().NotBeNull();
        result.CustomClass.GrandChild!.GrandChildId.Should().Be("I am the grandchild id");
    }

    [Fact]
    public void Deserialize_ChildClass_NotIncluded()
    {
        var result = ParentClassDocument.Deserialize(new Dictionary<string, AttributeValue>
        {
            {nameof(ParentClass.Id), new AttributeValue {S = "I am the root"}},
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ChildClassField_NotIncluded()
    {
        var result = ParentClassDocument.Deserialize(new Dictionary<string, AttributeValue>
        {
            {nameof(ParentClass.Id), new AttributeValue {S = "I am the root"}},
            {
                nameof(ParentClass.CustomClass), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>()
                }
            }
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().NotBeNull();
        result.CustomClass!.PropertyId.Should().BeNull();
        result.CustomClass.GrandChild.Should().BeNull();
    }

    [Fact]
    public void Deserialize_GrandChildClass_NotIncluded()
    {
        var result = ParentClassDocument.Deserialize(new Dictionary<string, AttributeValue>
        {
            {nameof(ParentClass.Id), new AttributeValue {S = "I am the root"}},
            {
                nameof(ParentClass.CustomClass), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {

                        {nameof(ParentClass.CustomClass.PropertyId), new AttributeValue {S = "I am the property"}}
                    }
                }
            }
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().NotBeNull();
        result.CustomClass!.PropertyId.Should().Be("I am the property");
        result.CustomClass.GrandChild.Should().BeNull();
    }

    [Fact]
    public void Deserialize_GrandChildClassField_NotIncluded()
    {
        var result = ParentClassDocument.Deserialize(new Dictionary<string, AttributeValue>
        {
            {nameof(ParentClass.Id), new AttributeValue {S = "I am the root"}},
            {
                nameof(ParentClass.CustomClass), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {

                        {nameof(ParentClass.CustomClass.PropertyId), new AttributeValue {S = "I am the property"}},
                        {
                            nameof(ParentClass.CustomClass.GrandChild), new AttributeValue()
                        }
                    }
                }
            }
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().NotBeNull();
        result.CustomClass!.PropertyId.Should().Be("I am the property");
        result.CustomClass.GrandChild.Should().NotBeNull();
        result.CustomClass.GrandChild!.GrandChildId.Should().BeNull();
    }
}

public class ParentClass
{
    public string? Id { get; set; }
    public ChildClass? CustomClass { get; set; }

    public class ChildClass
    {
        public string? PropertyId { get; set; }
        public GrandChildClass? GrandChild { get; set; }

        public class GrandChildClass
        {
            public string? GrandChildId { get; set; }
        }
    }
}