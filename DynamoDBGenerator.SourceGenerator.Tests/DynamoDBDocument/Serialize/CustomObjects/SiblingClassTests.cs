namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.CustomObjects;

[DynamoDBGenerator.DynamoDBDocument(typeof(SiblingClassOne))]
public partial class SiblingClassTests
{
    [Fact]
    public void Serialize_AllFieldsSet_AllIncluded()
    {
        var @class = new SiblingClassOne
        {
            Id = "I am the root",
            CustomClass = new SiblingClassTwo
            {
                PropertyId = "I am the property"
            }
        };

        SiblingClassOneDocument.Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(SiblingClassOne.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(SiblingClassOne.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        ((string)y.Key).Should().Be(nameof(SiblingClassTwo.PropertyId));
                        ((string)y.Value.S).Should().Be("I am the property");
                    });
                }
            );
    }

    [Fact]
    public void Serialize_CustomProperty_NotIncluded()
    {
        var @class = new SiblingClassOne
        {
            Id = "I am the root"
        };

        SiblingClassOneDocument.Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(SiblingClassOne.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                }
            );
    }

    [Fact]
    public void Serialize_CustomPropertyField_NotIncluded()
    {
        var @class = new SiblingClassOne
        {
            Id = "I am the root",
            CustomClass = new SiblingClassTwo()
        };

        SiblingClassOneDocument.Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(SiblingClassOne.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(SiblingClassOne.CustomClass));
                    x.Value.M.Should().BeEmpty();
                }
            );
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