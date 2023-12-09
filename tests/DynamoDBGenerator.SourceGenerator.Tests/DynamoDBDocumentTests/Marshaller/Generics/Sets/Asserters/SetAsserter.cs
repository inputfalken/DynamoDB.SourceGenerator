using System.ComponentModel;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

public abstract class SetAsserter<TSet, TElement> : MarshalAsserter<Container<TSet>> where TSet : IEnumerable<TElement>
{
    private readonly IEnumerable<TElement> _seed;
    private readonly Func<IEnumerable<TElement>, TSet> _fn;
    private static readonly bool IsStringSet = typeof(TElement) == typeof(string);


    protected override IEnumerable<(Container<TSet> element, Dictionary<string, AttributeValue> attributeValues)> Arguments()
    {
        yield return CreateArguments(_seed);
    }

    protected (Container<TSet> element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(IEnumerable<TElement> arg)
    {
        var res = _fn(arg);
        if (res is not IReadOnlySet<TElement> or not ISet<TElement>)
            throw new InvalidOperationException($"The type '{nameof(TSet)}' must either one of  'ISet<{nameof(TElement)}>, 'IReadonlySet<{nameof(TElement)}>'.");

        return (
            new Container<TSet>(res),
            new Dictionary<string, AttributeValue>
            {
                {
                    nameof(Container<TSet>.Element),
                    IsStringSet
                        ? new AttributeValue {SS = res.Select(x => x?.ToString()).ToList()}
                        : new AttributeValue {NS = res.Select(x => x?.ToString()).ToList()}
                }
            }
        );
    }

    protected SetAsserter(IEnumerable<TElement> seed, Func<IEnumerable<TElement>, TSet> fn)
    {
        _seed = seed;
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

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Container<TSet>.Element));
    }

    [Fact]
    public void Marshall_NoDuplicated_Elements()
    {
        Arguments().Should().AllSatisfy(x =>
        {
            var items = x.element.Element.Concat(x.element.Element).ToList();
            items.Should().HaveCountGreaterThan(0);
            var arguments = CreateArguments(items);

            MarshallImplementation(arguments.element).Should().SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(Container<TSet>.Element));
                if (IsStringSet)
                    x.Value.SS.Should().OnlyHaveUniqueItems();
                else
                    x.Value.NS.Should().OnlyHaveUniqueItems();
            });

        });
    }

}