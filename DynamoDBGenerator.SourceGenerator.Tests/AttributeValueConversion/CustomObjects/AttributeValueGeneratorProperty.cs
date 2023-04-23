namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.CustomObjects;

public class AttributeValueGeneratorProperty
{
    [Fact]
    public void BuildAttributeValues_AllFieldsSet_AllIncluded()
    {
        var @class = new RootClass
        {
            Id = "I am the root",
            CustomClass = new PropertyClass
            {
                PropertyId = "I am the property"
            }
        };

        @class.BuildAttributeValues()
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(RootClass.Id));
                    x.Value.S.Should().Be("I am the root");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(RootClass.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be(nameof(PropertyClass.PropertyId));
                        y.Value.S.Should().Be("I am the property");
                    });
                }
            );
    }

    [Fact]
    public void BuildAttributeValues_CustomProperty_NotIncluded()
    {
        var @class = new RootClass
        {
            Id = "I am the root"
        };

        @class.BuildAttributeValues()
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(RootClass.Id));
                    x.Value.S.Should().Be("I am the root");
                }
            );
    }

    [Fact]
    public void BuildAttributeValues_CustomPropertyField_NotIncluded()
    {
        var @class = new RootClass
        {
            Id = "I am the root",
            CustomClass = new PropertyClass()
        };

        @class.BuildAttributeValues()
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(RootClass.Id));
                    x.Value.S.Should().Be("I am the root");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(RootClass.CustomClass));
                    x.Value.M.Should().BeEmpty();
                }
            );
    }
}

[AttributeValueGenerator]
public partial class RootClass
{
    public string Id { get; set; }
    public PropertyClass CustomClass { get; set; }
}

[AttributeValueGenerator]
public partial class PropertyClass
{
    public string PropertyId { get; set; }
}