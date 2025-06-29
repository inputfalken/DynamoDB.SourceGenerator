using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

public abstract class MarshalAsserter<T>
{
    protected abstract IEnumerable<(T element, Dictionary<string, AttributeValue> attributeValues)> Arguments();
    protected abstract T UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues);
    protected abstract Dictionary<string, AttributeValue> MarshallImplementation(T element);

    [Fact]
    public void Marshall()
    {
        Arguments().Should()
            .AllSatisfy(x =>
            {
                var marshallImplementation = MarshallImplementation(x.element);
                marshallImplementation.Should().BeEquivalentTo(x.attributeValues);
            });
    }

    [Fact]
    public void Unmarshall()
    {
        Arguments().Should()
            .AllSatisfy(x =>
            {
                var unmarshallImplementation = UnmarshallImplementation(x.attributeValues);
                unmarshallImplementation.Should().BeEquivalentTo(x.element);
            });
    }

    [Fact]
    public void Marshall_IsEquivalentTo_UnmarshallResult()
    {
        Arguments().Should().AllSatisfy(x =>
        {
            var marshalImplementation = MarshallImplementation(x.element);
            var unmarshallImplementation = UnmarshallImplementation(marshalImplementation);
            unmarshallImplementation.Should().BeEquivalentTo(x.element);
        });
    }

    [Fact]
    public void UnMarshall_IsEquivalentTo_MarshallResult()
    {
        Arguments().Should().AllSatisfy(x =>
        {
            var unmarshallImplementation = UnmarshallImplementation(x.attributeValues);
            var marshallImplementation = MarshallImplementation(unmarshallImplementation);
            marshallImplementation.Should().BeEquivalentTo(x.attributeValues);
        });
    }

    [Fact]
    public void Unmarshall_NullAttributeValues_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}