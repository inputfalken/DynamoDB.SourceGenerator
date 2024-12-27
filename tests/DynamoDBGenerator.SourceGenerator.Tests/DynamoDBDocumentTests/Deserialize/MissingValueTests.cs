using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBMarshaller(EntityType = typeof(RequiredReferenceTypeValueMissingClass))]
[DynamoDBMarshaller(EntityType = typeof(OptionalReferenceTypeValueMissingClass))]
[DynamoDBMarshaller(EntityType = typeof(RequiredValueTypeValueMissingClass))]
public partial class MissingValueTests
{
    [Fact]
    public void Deserialize_RequiredReferenceTypeValueMissingClass_NoKeyValueProvidedShouldThrow()
    {
        var act = () =>
            RequiredReferenceTypeValueMissingClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>());
        act.Should().Throw<DynamoDBMarshallingException>();
    }

    [Fact]
    public void Deserialize_RequiredReferenceTypeValueMissingClass_KeyWithoutValueProvidedShouldThrow()
    {
        var act = () => RequiredReferenceTypeValueMissingClassMarshaller.Unmarshall(
            new Dictionary<string, AttributeValue> { { "RequiredProperty", new AttributeValue { S = null } } });
        act.Should().Throw<DynamoDBMarshallingException>();
    }

    [Fact]
    public void Deserialize_OptionalReferenceTypeValueMissingClass_KeyWithoutValueProvidedShouldNotThrow()
    {
        var act = () => OptionalReferenceTypeValueMissingClassMarshaller.Unmarshall(
            new Dictionary<string, AttributeValue> { { "OptionalProperty", new AttributeValue { S = null } } });
        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_OptionalReferenceTypeValueMissingClass_NoKeyValueProvidedShouldNotThrow()
    {
        var act = () =>
            OptionalReferenceTypeValueMissingClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>());

        act.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_RequiredValueTypeValueMissingClass_KeyWithoutValueProvidedShouldThrow()
    {
        var act = () => RequiredValueTypeValueMissingClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            { { "RequiredProperty", new AttributeValue { N = null } } });
        act.Should().Throw<DynamoDBMarshallingException>();
    }

    [Fact]
    public void Deserialize_RequiredValueTypeValueMissingClass_NoKeyValueProvidedShouldThrow()
    {
        var act = () =>
            RequiredValueTypeValueMissingClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>());
        act.Should().Throw<DynamoDBMarshallingException>().WithMessage(
            "The data member is not supposed to be null, to allow this; make the data member nullable. (Data member 'RequiredProperty')");
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