using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

public abstract class MarshalAsserter<T, TSeed> : MarshalAsserter<T>
{
    private readonly TSeed _seed;

    protected abstract (T element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(TSeed arg);

    protected override (T element, Dictionary<string, AttributeValue> attributeValues) Arguments() => CreateArguments(_seed);

    protected MarshalAsserter(TSeed seed)
    {
        _seed = seed;
    }


}
public abstract class MarshalAsserter<T>
{
    protected abstract T UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues);
    protected abstract Dictionary<string, AttributeValue> MarshallImplementation(T element);

    protected abstract (T element, Dictionary<string, AttributeValue> attributeValues) Arguments();


    [Fact]
    public void Marshall()
    {
        var (element, attributeValues) = Arguments();
        MarshallImplementation(element).Should().BeEquivalentTo(attributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        var (element, dict) = Arguments();
        UnmarshallImplementation(dict).Should().BeEquivalentTo(element);
    }

    [Fact]
    public void Unmarshall_NullAttributeValues_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(null!);

        act.Should().Throw<ArgumentNullException>();
    }

}
