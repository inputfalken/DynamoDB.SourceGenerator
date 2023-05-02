namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class TupleTests
{
    [Fact]
    public void BuildAttributeValues_XY_Property()
    {
        var @class = new TupleClass
        {
            XYCoordinate = (1, 2),
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x =>
            {
                x.Key.Should().Be(nameof(TupleClass.XYCoordinate));
                x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be("X");
                        y.Value.N.Should().Be("1");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Y");
                        y.Value.N.Should().Be("2");
                    }
                );
            }
        );
    }

    [Fact]
    public void BuildAttributeValues_XYZ_Property()
    {
        var @class = new TupleClass
        {
            XYZCoordinate = (3, 4, 5)
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x =>
            {
                x.Key.Should().Be(nameof(TupleClass.XYZCoordinate));
                x.Value.M.Should().SatisfyRespectively(
                    y =>
                    {
                        y.Key.Should().Be("X");
                        y.Value.N.Should().Be("3");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Y");
                        y.Value.N.Should().Be("4");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Z");
                        y.Value.N.Should().Be("5");
                    }
                );
            }
        );
    }

    [Fact]
    public void BuildAttributeValues_Both_Properties()
    {
        var @class = new TupleClass
        {
            XYCoordinate = (1, 2),
            XYZCoordinate = (3, 4, 5)
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x =>
            {
                x.Key.Should().Be(nameof(TupleClass.XYCoordinate));
                x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be("X");
                        y.Value.N.Should().Be("1");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Y");
                        y.Value.N.Should().Be("2");
                    }
                );
            },
            x =>
            {
                x.Key.Should().Be(nameof(TupleClass.XYZCoordinate));
                x.Value.M.Should().SatisfyRespectively(
                    y =>
                    {
                        y.Key.Should().Be("X");
                        y.Value.N.Should().Be("3");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Y");
                        y.Value.N.Should().Be("4");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Z");
                        y.Value.N.Should().Be("5");
                    }
                );
            }
        );
    }
}

[AttributeValueGenerator]
public partial class TupleClass
{
    public (int X, int Y)? XYCoordinate { get; set; }
    public (int X, int Y, int Z)? XYZCoordinate { get; set; }
}