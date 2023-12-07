using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller;

public abstract class MarshalAsserter<T, TSeed>
{
    private readonly TSeed _seed;

    protected abstract (T element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(TSeed arg);

    protected (T element, Dictionary<string, AttributeValue> attributeValues) DefaultArguments => CreateArguments(_seed);
    protected abstract Dictionary<string, AttributeValue> MarshallImplementation(T element);
    protected abstract T UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues);

    protected MarshalAsserter(TSeed seed)
    {
        _seed = seed;
    }

    [Fact]
    public void Marshall()
    {
        var (element, attributeValues) = CreateArguments(_seed);
        MarshallImplementation(element).Should().BeEquivalentTo(attributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        var (element, dict) = CreateArguments(_seed);
        UnmarshallImplementation(dict).Should().BeEquivalentTo(element);
    }

}