namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.CustomObjects;

public class SiblingClassTests
{
    [Fact]
    public void BuildAttributeValues_AllFieldsSet_AllIncluded()
    {
        var @class = new SiblingClassOne
        {
            Id = "I am the root",
            CustomClass = new SiblingClassTwo
            {
                PropertyId = "I am the property"
            }
        };

        @class.BuildAttributeValues()
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
    public void BuildAttributeValues_CustomProperty_NotIncluded()
    {
        var @class = new SiblingClassOne
        {
            Id = "I am the root"
        };

        @class.BuildAttributeValues()
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
    public void BuildAttributeValues_CustomPropertyField_NotIncluded()
    {
        var @class = new SiblingClassOne
        {
            Id = "I am the root",
            CustomClass = new SiblingClassTwo()
        };

        @class.BuildAttributeValues()
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

[AttributeValueGenerator]
public partial class SiblingClassOne
{
    public string Id { get; set; }
    public SiblingClassTwo CustomClass { get; set; }
}

public class SiblingClassTwo
{
    public string PropertyId { get; set; }
}