using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Tuples;

[DynamoDBMarshaller(EntityType = typeof((string FirstName, string LastName, string? EmailAddress)))]
public partial class SimpleTupleTests : MarshalAsserter<(string FirstName, string LastName, string? EmailAddress)>
{
    private static readonly Fixture Fixture = new();

    private static ((string, string, string? ), Dictionary<string, AttributeValue>) Av(
        (string FirstName, string LastName, string? EmailAddress) tuple)
    {
        var dict = new Dictionary<string, AttributeValue>
        {
            { nameof(tuple.FirstName), new AttributeValue { S = tuple.FirstName } },
            { nameof(tuple.LastName), new AttributeValue { S = tuple.LastName } }
        };

        if (tuple.EmailAddress is null)
            return (tuple, dict);

        dict[nameof(tuple.EmailAddress)] = new AttributeValue { S = tuple.EmailAddress };

        return (tuple, dict);
    }

    [Theory]
    [InlineData(null, null, null, "FirstName")]
    [InlineData(null, "ABC", null, "FirstName")]
    [InlineData("ABC", null, null, "LastName")]
    public void Marshall_NullField_Throws(string? firstName, string? lastName, string? emailAddress, string dataMember)
    {
        var (valueTuple, _) = Av((firstName!, lastName!, emailAddress));

        var act = () => ValueTupleMarshaller.Marshall(valueTuple);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(dataMember);
    }

    [Theory]
    [InlineData(null, null, null, "FirstName")]
    [InlineData(null, "ABC", null, "FirstName")]
    [InlineData("ABC", null, null, "LastName")]
    public void Unmarshall_NullField_Throws(string? firstName, string? lastName, string? emailAddress,
        string? dataMember)
    {
        var (_, _attributeValues) = Av((firstName!, lastName!, emailAddress));

        var act = () => ValueTupleMarshaller.Unmarshall(_attributeValues);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(dataMember);
    }

    protected override IEnumerable<((string FirstName, string LastName, string? EmailAddress) element,
        Dictionary<string, AttributeValue> attributeValues)> Arguments()
    {
        var builder = Fixture.Build<(string FirstName, string LastName, string? EmailAddress)>();

        yield return Av(builder.Create());
        yield return Av(builder.With(x => x.EmailAddress, (string?)null).Create());
    }

    protected override (string FirstName, string LastName, string? EmailAddress) UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ValueTupleMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(
        (string FirstName, string LastName, string? EmailAddress) element)
    {
        return ValueTupleMarshaller.Marshall(element);
    }
}