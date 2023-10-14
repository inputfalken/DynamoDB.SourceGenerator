using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(typeof(LookUpClass))]
public partial class LookUpTests
{
    [Fact]
    public void Deserialize_EmptyLookup_IsIncluded()
    {
        LookUpClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(LookUpClass.Lookup), new AttributeValue {M = new Dictionary<string, AttributeValue>()}}})
            .Should()
            .BeOfType<LookUpClass>()
            .Which
            .Lookup
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Deserialize_LookupWithValues_IsIncluded()
    {
        LookUpClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(LookUpClass.Lookup), new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {"first", new AttributeValue {L = new List<AttributeValue> {new() {N = "1"}, new() {N = "2"}}}},
                            {"second", new AttributeValue {L = new List<AttributeValue> {new() {N = "3"}, new() {N = "4"}}}}
                        }
                    }
                }
            })
            .Should()
            .BeOfType<LookUpClass>()
            .Which
            .Lookup
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be("first");
                x.Should().SatisfyRespectively(y => y.Should().Be(1), y => y.Should().Be(2));
            }, x =>
            {

                x.Key.Should().Be("second");
                x.Should().SatisfyRespectively(y => y.Should().Be(3), y => y.Should().Be(4));
            });
    }
}

public class LookUpClass
{
    [DynamoDBProperty]
    public ILookup<string, int>? Lookup { get; set; }
}