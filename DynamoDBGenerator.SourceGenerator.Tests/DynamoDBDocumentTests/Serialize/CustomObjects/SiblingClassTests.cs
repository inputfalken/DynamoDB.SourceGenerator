namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.CustomObjects;

[DynamoDBMarshaller(typeof(SiblingClassOne))]
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

        SiblingClassOneMarshaller.Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(SiblingClassOne.Id));
                    x.Value.S.Should().Be("I am the root");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(SiblingClassOne.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be(nameof(SiblingClassTwo.PropertyId));
                        y.Value.S.Should().Be("I am the property");
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

        SiblingClassOneMarshaller.Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(SiblingClassOne.Id));
                    x.Value.S.Should().Be("I am the root");
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

        SiblingClassOneMarshaller.Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(SiblingClassOne.Id));
                    x.Value.S.Should().Be("I am the root");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(SiblingClassOne.CustomClass));
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