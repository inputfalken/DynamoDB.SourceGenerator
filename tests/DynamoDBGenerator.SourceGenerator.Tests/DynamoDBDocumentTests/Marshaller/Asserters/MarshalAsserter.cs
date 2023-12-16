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
        Arguments().Should().AllSatisfy(x => MarshallImplementation(x.element).Should().BeEquivalentTo(x.attributeValues));
    }

    [Fact]
    public void Unmarshall()
    {
        Arguments().Should().AllSatisfy(x => UnmarshallImplementation(x.attributeValues).Should().BeEquivalentTo(x.element));
    }

    [Fact]
    public void Marshall_IsEquivalentTo_UnmarshallResult()
    {
        Arguments().Should().AllSatisfy(x => UnmarshallImplementation(MarshallImplementation(x.element)).Should().BeEquivalentTo(x.element));

    }
    
    [Fact]
    public void UnMarshall_IsEquivalentTo_MarshallResult()
    {
        Arguments().Should().AllSatisfy(x => MarshallImplementation(UnmarshallImplementation(x.attributeValues)).Should().BeEquivalentTo(x.attributeValues));
    }

    [Fact]
    public void Unmarshall_NullAttributeValues_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}