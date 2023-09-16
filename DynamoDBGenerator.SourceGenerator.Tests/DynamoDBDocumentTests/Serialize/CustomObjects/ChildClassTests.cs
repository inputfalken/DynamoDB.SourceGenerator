using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.CustomObjects;

[DynamoDBMarshaller(typeof(ParentClass))]
public partial class ChildClassTests
{
    [Fact]
    public void Serialize_AllFieldsSet_AllIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root",
            CustomClass = new ParentClass.ChildClass
            {
                PropertyId = "I am the property",
                GrandChild = new ParentClass.ChildClass.GrandChildClass
                {
                    GrandChildId = "I am the grandchild id"
                }
            }
        };

        ParentClassMarshaller.Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(ParentClass.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                        {
                            ((string)y.Key).Should().Be(nameof(ParentClass.ChildClass.PropertyId));
                            ((string)y.Value.S).Should().Be("I am the property");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be(nameof(ParentClass.ChildClass.GrandChild));
                            y.Value.M.Should().SatisfyRespectively(z =>
                            {
                                ((string)z.Key).Should().Be(nameof(ParentClass.ChildClass.GrandChildClass.GrandChildId));
                                ((string)z.Value.S).Should().Be("I am the grandchild id");
                            });
                        }
                    );
                }
            );
    }

    [Fact]
    public void Serialize_ChildClass_NotIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root"
        };

        ParentClassMarshaller.Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                }
            );
    }

    [Fact]
    public void Serialize_ChildClassField_NotIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root",
            CustomClass = new ParentClass.ChildClass()
        };

        ParentClassMarshaller.Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(ParentClass.CustomClass));
                    x.Value.M.Should().BeEmpty();
                }
            );
    }

    [Fact]
    public void Serialize_GrandChildClass_NotIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root",
            CustomClass = new ParentClass.ChildClass
            {
                PropertyId = "Abc"
            }
        };

        ParentClassMarshaller.Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(ParentClass.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        ((string)y.Key).Should().Be(nameof(ParentClass.CustomClass.PropertyId));
                        ((string)y.Value.S).Should().Be("Abc");
                    });
                }
            );
    }

    [Fact]
    public void Serialize_GrandChildClassField_NotIncluded()
    {
        var @class = new ParentClass
        {
            Id = "I am the root",
            CustomClass = new ParentClass.ChildClass
            {
                GrandChild = new ParentClass.ChildClass.GrandChildClass(),
                PropertyId = "123"
            }
        };

        ParentClassMarshaller.Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ParentClass.Id));
                    ((string)x.Value.S).Should().Be("I am the root");
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(ParentClass.CustomClass));
                    x.Value.M.Should().SatisfyRespectively(y =>
                        {
                            ((string)y.Key).Should().Be(nameof(ParentClass.CustomClass.PropertyId));
                            ((string)y.Value.S).Should().Be("123");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be(nameof(ParentClass.CustomClass.GrandChild));
                            y.Value.M.Should().BeEmpty();
                        }
                    );
                }
            );
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
