using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(typeof(OptionalIntegerClass))]
public partial class NullableTests
{
    [Fact]
    public void Deserialize_NoValueMapped_ShouldThrow()
    {
        // If we have an AttributeValue that can not be be mapped when we have the key, then we should throw.
        var act = () => OptionalIntegerClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue> {{"OptionalProperty", new AttributeValue {N = null}}});
        act.Should().Throw<DynamoDBMarshallingException>();
    }

    [Fact]
    public void Deserialize_NoValueProvided_ShouldNotThrow()
    {
        var act = () => OptionalIntegerClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>());
        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_KeyValueProvided_ShouldNotThrow()
    {
        OptionalIntegerClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{"OptionalProperty", new AttributeValue {N = "2"}}})
            .OptionalProperty
            .Should()
            .Be(2);
    }
}

public class OptionalIntegerClass
{
    public int? OptionalProperty { get; set; }
}