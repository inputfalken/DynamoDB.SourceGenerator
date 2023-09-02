using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshallert(typeof(OptionalIntegerClass))]
public partial class NullableTests
{
    [Fact]
    public void Deserialize_NoValueProvided_ShouldNotThrow()
    {
        var act = () => OptionalIntegerClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"OptionalProperty", new AttributeValue {N = null}}});
        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_NoKeyValueProvided_ShouldNotThrow()
    {
        var act = () => OptionalIntegerClassDocument.Deserialize(new Dictionary<string, AttributeValue>());
        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_KeyValueProvided_ShouldNotThrow()
    {
        OptionalIntegerClassDocument
            .Deserialize(new Dictionary<string, AttributeValue> {{"OptionalProperty", new AttributeValue {N = "2"}}})
            .OptionalProperty
            .Should()
            .Be(2);
    }
}

public class OptionalIntegerClass
{
    public int? OptionalProperty { get; set; }
}