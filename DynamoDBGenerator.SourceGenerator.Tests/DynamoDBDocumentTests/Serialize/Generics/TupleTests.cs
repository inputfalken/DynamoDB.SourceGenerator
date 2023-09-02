namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBMarshallert(typeof(TupleClass))]
public partial class TupleTests
{

    [Fact]
    public void Serialize_Both_Properties()
    {
        var @class = new TupleClass
        {
            XYCoordinate = (1, 2),
            XYZCoordinate = (3, 4, 5)
        };

        TupleClassDocument
            .Serialize(@class)
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

        TupleClassDocument
            .Serialize(@class)
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

        TupleClassDocument
            .Serialize(@class)
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
