using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

public abstract class NullableElementAsserter<TCollection, TElement> : CollectionAsserter<TCollection, TElement> where TCollection : IEnumerable<TElement> where TElement : class?
{

    protected static IEnumerable<string> Strings()
    {
        yield return "foo";
        yield return "bar";
        yield return "john";
        yield return "doe";
        yield return "";
    }
    protected NullableElementAsserter(IEnumerable<TElement> seed, Func<IEnumerable<TElement>, TCollection> fn) : base(seed, x => x is null ? new AttributeValue {NULL = true} : new AttributeValue {S = x.ToString()}, fn)
    {
    }

    [Fact]
    public void Marshall_NullElement_ShouldAddNullAttributeValue()
    {
        var defaultArgs = Arguments();

        var items = defaultArgs.element.Rows.Append(null).ToList();
        var args = CreateArguments(items!);

        MarshallImplementation(args.element).Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(Text<TCollection>.Rows));
            x.Value.L.Should().NotBeNullOrEmpty();
            x.Value.L.Should().HaveSameCount(items);
            x.Value.L[items.Count - 1].Should().BeEquivalentTo(new AttributeValue {NULL = true});
        });
    }
    
    [Fact]
    public void Unmarshall_NullElement_ShouldAddNullAttributeValue()
    {
        var defaultArgs = Arguments();

        var items = defaultArgs.element.Rows.Append(null).ToList();
        var args = CreateArguments(items!);

        var collection = UnmarshallImplementation(args.attributeValues).Rows.ToList();

        args.attributeValues.Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(Text<TCollection>.Rows));
            x.Value.L.Should().NotBeNullOrEmpty();
            x.Value.L.Should().HaveSameCount(collection);

            collection[x.Value.L.Count - 1].Should().BeNull();
        });

    }
}