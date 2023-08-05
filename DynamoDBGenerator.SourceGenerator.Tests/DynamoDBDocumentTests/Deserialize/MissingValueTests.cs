using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBDocument(typeof(RequiredReferenceTypeValueMissingClass))]
[DynamoDBDocument(typeof(OptionalReferenceTypeValueMissingClass))]
[DynamoDBDocument(typeof(RequiredValueTypeValueMissingClass))]
public partial class MissingValueTests
{
    [Fact]
    public void Deserialize_RequiredReferenceTypeValueMissingClass_NoKeyValueProvidedShouldThrow()
    {
        var act = () => RequiredReferenceTypeValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_RequiredReferenceTypeValueMissingClass_KeyWithoutValueProvidedShouldThrow()
    {
        var act = () => RequiredReferenceTypeValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"RequiredProperty", new AttributeValue {S = null}}});
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void Deserialize_OptionalReferenceTypeValueMissingClass_KeyWithoutValueProvidedShouldNotThrow()
    {
        var act = () => OptionalReferenceTypeValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"OptionalProperty", new AttributeValue {S = null}}});
        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_OptionalReferenceTypeValueMissingClass_NoKeyValueProvidedShouldNotThrow()
    {
        var act = () => OptionalReferenceTypeValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue>());

        act.Should().NotThrow();
    }
    [Fact]
    public void Deserialize_RequiredValueTypeValueMissingClass_KeyWithoutValueProvidedShouldThrow()
    {
        var act = () => RequiredValueTypeValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue> {{"RequiredProperty", new AttributeValue {N = null}}});
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_RequiredValueTypeValueMissingClass_NoKeyValueProvidedShouldThrow()
    {
        var act = () => RequiredValueTypeValueMissingClassDocument.Deserialize(new Dictionary<string, AttributeValue>());
        act.Should().Throw<ArgumentNullException>();
    }
}

public class RequiredReferenceTypeValueMissingClass
{
    public string RequiredProperty { get; set; } = null!;
}

public class OptionalReferenceTypeValueMissingClass
{
    public string? OptionalProperty { get; set; }
}

public class RequiredValueTypeValueMissingClass
{
    public int RequiredProperty { get; set; }
}