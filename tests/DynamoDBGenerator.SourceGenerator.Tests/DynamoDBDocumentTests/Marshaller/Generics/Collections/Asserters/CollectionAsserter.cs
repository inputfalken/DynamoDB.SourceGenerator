using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

public abstract class CollectionAsserter<TCollection, TElement> : MarshalAsserter<Container<TCollection>, IEnumerable<TElement>> where TCollection : IEnumerable<TElement>
{
    private readonly Func<TElement, AttributeValue> _av;

    private readonly Func<IEnumerable<TElement>, TCollection> _fn;

    protected override sealed (Container<TCollection> element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(IEnumerable<TElement> arg)
    {
        var items = arg.ToList();
        var element = new Container<TCollection>(_fn(items));
        return (
            element,
            new()
            {
                {
                    nameof(Container<TCollection>.Element), new AttributeValue {L = items.Select(_av).ToList()}
                }
            }
        );
    }


    protected CollectionAsserter(IEnumerable<TElement> seed, Func<TElement, AttributeValue> av, Func<IEnumerable<TElement>, TCollection> fn) : base(seed)
    {
        _av = av;
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

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Container<TCollection>.Element));
    }

}