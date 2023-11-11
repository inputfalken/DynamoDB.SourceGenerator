using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(typeof(TupleClass))]
[DynamoDBMarshaller(typeof((int X, int Y)), PropertyName = "XAndYTuple")]
[DynamoDBMarshaller(typeof((int, int )), PropertyName = "UnnamedTuple")]
[DynamoDBMarshaller(typeof((string Id, (int X, int Y) Coordinates)), PropertyName = "NestedXAndYTuple")]
public partial class TupleTests
{
    [Fact]
    public void Deserialize_UnnamedTuple_Properties()
    {
        UnnamedTuple
            .Unmarshall(new Dictionary<string, AttributeValue> {{"Item1", new AttributeValue {N = "1"}}, {"Item2", new AttributeValue {N = "2"}}})
            .Should()
            .Be((1, 2));
    }

    [Fact]
    public void Deserialize_NestedXAndYTuple_Properties()
    {
        NestedXAndYTuple
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {"Id", new AttributeValue {S = "123"}},
                {
                    "Coordinates", new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>()
                        {
                            {"X", new AttributeValue {N = "1"}},
                            {"Y", new AttributeValue {N = "2"}}
                        }
                    }
                }
            })
            .Should()
            .Be(("123", (1, 2)));
    }
    [Fact]
    public void Deserialize_XAndYTuple_Properties()
    {
        XAndYTuple
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {"X", new AttributeValue {N = "1"}},
                {"Y", new AttributeValue {N = "2"}}
            })
            .Should()
            .Be((1, 2));
    }

    [Fact]
    public void Deserialize_Both_Properties()
    {
        var result = TupleClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    "XYCoordinate", new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {

                            {"X", new AttributeValue {N = "1"}},
                            {"Y", new AttributeValue {N = "2"}}
                        }
                    }
                },
                {
                    "XYZCoordinate", new AttributeValue
                    {

                        M = new Dictionary<string, AttributeValue>
                        {

                            {"X", new AttributeValue {N = "3"}},
                            {"Y", new AttributeValue {N = "4"}},
                            {"Z", new AttributeValue {N = "5"}}
                        }
                    }
                }
            });

        result.XYCoordinate.Should().Be((1, 2));
        result.XYZCoordinate.Should().Be((3, 4, 5));
    }
    [Fact]
    public void Deserialize_XY_Property()
    {
        var result = TupleClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    "XYCoordinate", new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {

                            {"X", new AttributeValue {N = "1"}},
                            {"Y", new AttributeValue {N = "2"}}
                        }
                    }
                },
            });

        result.XYCoordinate.Should().Be((1, 2));
        result.XYZCoordinate.Should().Be(null);
    }

    [Fact]
    public void Deserialize_XYZ_Property()
    {
        var result = TupleClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    "XYZCoordinate", new AttributeValue
                    {

                        M = new Dictionary<string, AttributeValue>
                        {

                            {"X", new AttributeValue {N = "3"}},
                            {"Y", new AttributeValue {N = "4"}},
                            {"Z", new AttributeValue {N = "5"}}
                        }
                    }
                }
            });
        result.XYCoordinate.Should().BeNull();
        result.XYZCoordinate.Should().Be((3, 4, 5));
    }
}

public class TupleClass
{
    public (int X, int Y)? XYCoordinate { get; set; }
    public (int X, int Y, int Z)? XYZCoordinate { get; set; }
}