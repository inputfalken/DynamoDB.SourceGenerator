using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

public abstract class CollectionAsserter<TCollection, TElement> : MarshalAsserter<CollectionAsserter<TCollection, TElement>.Text, IEnumerable<TElement>> where TCollection : IEnumerable<TElement>?
{

    private readonly Func<IEnumerable<TElement>, TCollection> _fn;

    public record Text(TCollection Rows);

    protected override sealed (Text element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(IEnumerable<TElement> arg)
    {
        var items = arg.ToList();
        var element = new Text(_fn(items));
        return (
            element,
            new()
            {
                {
                    nameof(Text.Rows), new AttributeValue {L = items.Select(x => new AttributeValue {S = x?.ToString()}).ToList()}
                }
            }
        );
    }


    protected CollectionAsserter(IEnumerable<TElement> seed, Func<IEnumerable<TElement>, TCollection> fn) : base(seed)
    {
        _fn = fn;
    }


    [Fact]
    public void Marshall_Empty()
    {
        var (text, attributeValues) = CreateArguments(Enumerable.Empty<TElement>());
        MarshallImplementation(text).Should().BeEquivalentTo(attributeValues);
    }

    [Fact]
    public void Unmarshall_Empty()
    {
        var (text, attributeValues) = CreateArguments(Enumerable.Empty<TElement>());
        UnmarshallImplementation(attributeValues).Should().BeEquivalentTo(text);
    }

    [Fact]
    public void Unmarshall_EmptyAttributeValues_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(new Dictionary<string, AttributeValue>());

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Text.Rows));
    }
    
}