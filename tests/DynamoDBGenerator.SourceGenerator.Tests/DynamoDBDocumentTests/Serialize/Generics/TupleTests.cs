using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBMarshaller(typeof(TupleClass))]
[DynamoDBMarshaller(typeof((int X, int Y)), PropertyName = "XAndYTuple")]
[DynamoDBMarshaller(typeof((int, int )), PropertyName = "UnnamedTuple")]
[DynamoDBMarshaller(typeof((string Id, (int X, int Y) Coordinates)), PropertyName = "NestedXAndYTuple")]
public partial class TupleTests
{
    [Fact]
    public void Serialize_UnnamedTuple_Properties()
    {
        UnnamedTuple.Marshall((1, 2))
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be("Item1");
                    x.Value.N.Should().Be("1");
                }, x =>
                {

                    x.Key.Should().Be("Item2");
                    x.Value.N.Should().Be("2");
                }
            );

    }

    [Fact]
    public void Serialize_NestedXAndYTuple_Properties()
    {
        NestedXAndYTuple
            .Marshall(("123", (1, 2)))
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be("Id");
                    x.Value.S.Should().Be("123");
                }, static x =>
                {
                    x.Key.Should().Be("Coordinates");
                    x.Value.M
                        .Should()
                        .SatisfyRespectively(x =>
                        {
                            x.Key.Should().Be("X");
                            x.Value.N.Should().Be("1");
                        }, x =>
                        {
                            x.Key.Should().Be("Y");
                            x.Value.N.Should().Be("2");
                        });
                }
            );
    }
    [Fact]
    public void Serialize_XAndYTuple_Properties()
    {
        XAndYTuple
            .Marshall((1, 2))
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be("X");
                    x.Value.N.Should().Be("1");
                }, x =>
                {
                    x.Key.Should().Be("Y");
                    x.Value.N.Should().Be("2");
                }
            );
    }

    [Fact]
    public void Serialize_Both_Properties()
    {
        var @class = new TupleClass
        {
            XYCoordinate = (1, 2),
            XYZCoordinate = (3, 4, 5)
        };

        TupleClassMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(TupleClass.XYCoordinate));
                    x.Value.M.Should().SatisfyRespectively(y =>
                        {
                            ((string)y.Key).Should().Be("X");
                            ((string)y.Value.N).Should().Be("1");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Y");
                            ((string)y.Value.N).Should().Be("2");
                        }
                    );
                },
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(TupleClass.XYZCoordinate));
                    x.Value.M.Should().SatisfyRespectively(
                        y =>
                        {
                            ((string)y.Key).Should().Be("X");
                            ((string)y.Value.N).Should().Be("3");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Y");
                            ((string)y.Value.N).Should().Be("4");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Z");
                            ((string)y.Value.N).Should().Be("5");
                        }
                    );
                }
            );
    }
    [Fact]
    public void Serialize_XY_Property()
    {
        var @class = new TupleClass
        {
            XYCoordinate = (1, 2)
        };

        TupleClassMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(TupleClass.XYCoordinate));
                    x.Value.M.Should().SatisfyRespectively(y =>
                        {
                            ((string)y.Key).Should().Be("X");
                            ((string)y.Value.N).Should().Be("1");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Y");
                            ((string)y.Value.N).Should().Be("2");
                        }
                    );
                }
            );
    }

    [Fact]
    public void Serialize_XYZ_Property()
    {
        var @class = new TupleClass
        {
            XYZCoordinate = (3, 4, 5)
        };

        TupleClassMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(TupleClass.XYZCoordinate));
                    x.Value.M.Should().SatisfyRespectively(
                        y =>
                        {
                            ((string)y.Key).Should().Be("X");
                            ((string)y.Value.N).Should().Be("3");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Y");
                            ((string)y.Value.N).Should().Be("4");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Z");
                            ((string)y.Value.N).Should().Be("5");
                        }
                    );
                }
            );
    }
}

public class TupleClass
{
    public (int X, int Y)? XYCoordinate { get; set; }
    public (int X, int Y, int Z)? XYZCoordinate { get; set; }
}