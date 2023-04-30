namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.CustomObjects;

public class ChildClassTests
{
    [Fact]
    public void BuildAttributeValues_AllFieldsSet_AllIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root",
            CustomClass = new ParentClass.ChildClass
            {
                PropertyId = "I am the property",
                GrandChild = new ParentClass.ChildClass.GrandChildClass()
                {
                    GrandChildId = "I am the grandchild id"
                }
            }
        };

        @class.BuildAttributeValues()
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    x.Value.S.Should().Be("I am the root");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                        {
                            y.Key.Should().Be(nameof(ParentClass.ChildClass.PropertyId));
                            y.Value.S.Should().Be("I am the property");
                        },
                        y =>
                        {
                            y.Key.Should().Be(nameof(ParentClass.ChildClass.GrandChild));
                            y.Value.M.Should().SatisfyRespectively(z =>
                            {
                                z.Key.Should().Be(nameof(ParentClass.ChildClass.GrandChildClass.GrandChildId));
                                z.Value.S.Should().Be("I am the grandchild id");
                            });

                        }
                    );
                }
            );
    }

    [Fact]
    public void BuildAttributeValues_CustomProperty_NotIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root"
        };

        @class.BuildAttributeValues()
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    x.Value.S.Should().Be("I am the root");
                }
            );
    }

    [Fact]
    public void BuildAttributeValues_CustomPropertyField_NotIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root",
            CustomClass = new ParentClass.ChildClass()
        };

        @class.BuildAttributeValues()
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    x.Value.S.Should().Be("I am the root");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.CustomClass));
                    x.Value.M.Should().BeEmpty();
                }
            );
    }
}

[AttributeValueGenerator]
public partial class ParentClass
{
    public string Id { get; set; }
    public ChildClass CustomClass { get; set; }

    public class ChildClass
    {
        public string PropertyId { get; set; }
        public GrandChildClass GrandChild { get; set; }

        public class GrandChildClass
        {
            public string GrandChildId { get; set; }
        }
    }
}