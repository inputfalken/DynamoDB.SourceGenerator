using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBDocument(typeof(OptionalIntegerClass))]
public partial class NullableTests
{

    [Fact]
    public void Deserialize_NoValueProvided_ShouldNotThrow()
    {
        var act = () => OptionalIntegerClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"RequiredProperty", new AttributeValue {N = null}}});
        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_NoKeyValueProvided_ShouldNotThrow()
    {
        var act = () => OptionalIntegerClassDocument.Deserialize(new Dictionary<string, AttributeValue>());
        act.Should().NotThrow();
    }

}

public class OptionalIntegerClass
{
    public int? RequiredProperty { get; set; }
}