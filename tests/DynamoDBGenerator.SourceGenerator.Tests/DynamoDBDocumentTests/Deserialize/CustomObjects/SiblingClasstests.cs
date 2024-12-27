using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.CustomObjects;

[DynamoDBMarshaller(EntityType = typeof(SiblingClassOne))]
public partial class SiblingClassTests
{
    [Fact]
    public void Serialize_AllFieldsSet_AllIncluded()
    {
        var result = SiblingClassOneMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                { nameof(SiblingClassOne.Id), new AttributeValue { S = "I am the root" } },

                {
                    nameof(SiblingClassOne.CustomClass), new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {
                                nameof(SiblingClassOne.CustomClass.PropertyId), new AttributeValue
                                    { S = "I am the property" }
                            }
                        }
                    }
                }
            });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().NotBeNull();
        result.CustomClass!.PropertyId.Should().Be("I am the property");
    }

    [Fact]
    public void Serialize_CustomProperty_NotIncluded()
    {
        var result = SiblingClassOneMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            { nameof(SiblingClassOne.Id), new AttributeValue { S = "I am the root" } }
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().BeNull();
    }

    [Fact]
    public void Serialize_CustomPropertyField_NotIncluded()
    {
        var result = SiblingClassOneMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            { nameof(SiblingClassOne.Id), new AttributeValue { S = "I am the root" } },

            {
                nameof(SiblingClassOne.CustomClass), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>()
                }
            }
        });

        result.Id.Should().Be("I am the root");
        result.CustomClass.Should().NotBeNull();
        result.CustomClass!.PropertyId.Should().BeNull();
    }
}

public class SiblingClassOne
{
    public string? Id { get; set; }
    public SiblingClassTwo? CustomClass { get; set; }
}

public class SiblingClassTwo
{
    public string? PropertyId { get; set; }
}