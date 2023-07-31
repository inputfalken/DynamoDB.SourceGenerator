using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBDocument(typeof(RequiredValueMissingClass))]
[DynamoDBDocument(typeof(OptionalValueMissingClass))]
public partial class MissingValueTests
{
    [Fact]
    public void Deserialize__RequiredValueMissingClass_NoKeyValueProvidedShouldThrow()
    {
        var act = () => RequiredValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue>());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_RequiredValueMissingClass_KeyWithoutValueProvidedShouldThrow()
    {
        var act = () => RequiredValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"RequiredProperty", new AttributeValue {S = null}}});
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_OptionalValueMissingClass_KeyWithoutValueProvidedShouldNotThrow()
    {
        var act = () => OptionalValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"OptionalProperty", new AttributeValue {S = null}}});
        act.Should().NotThrow();
    }
    
    [Fact]
    public void Deserialize_OptionalValueMissingClass__NoKeyValueProvidedShouldThrow()
    {
        var act = () => OptionalValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue>());

        act.Should().NotThrow();
    }
}

public class RequiredValueMissingClass
{
    public string RequiredProperty { get; set; } = null!;
}

public class OptionalValueMissingClass
{
    public string? OptionalProperty { get; set; }
}