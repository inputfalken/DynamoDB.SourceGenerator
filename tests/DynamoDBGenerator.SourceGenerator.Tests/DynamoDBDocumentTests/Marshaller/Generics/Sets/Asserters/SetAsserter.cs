using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

public abstract class SetAsserter<TSet, TElement> : MarshalAsserter<SetAsserter<TSet, TElement>.NameList, IEnumerable<TElement>> where TSet : IEnumerable<TElement>
{
    private readonly Func<IEnumerable<TElement>, TSet> _fn;

    private static readonly bool IsStringSet = typeof(TElement) == typeof(string);

    public record NameList(TSet UniqueNames);


    protected override (NameList element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(IEnumerable<TElement> arg)
    {
        var res = _fn(arg);

        return (
            new NameList(res),
            new Dictionary<string, AttributeValue>
            {
                {
                    nameof(NameList.UniqueNames),
                    IsStringSet
                        ? new AttributeValue {SS = res.Select(x => x?.ToString()).ToList()}
                        : new AttributeValue {NS = res.Select(x => x?.ToString()).ToList()}
                }
            }
        );
    }

    protected SetAsserter(IEnumerable<TElement> seed, Func<IEnumerable<TElement>, TSet> fn) : base(seed)
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

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(NameList.UniqueNames));
    }

    [Fact]
    public void Marshall_NoDuplicated_Elements()
    {
        var (nameList, _) = DefaultArguments;
        var items = nameList.UniqueNames.Concat(nameList.UniqueNames).ToList();

        items.Should().HaveCountGreaterThan(0);
        var arguments = CreateArguments(items);

        MarshallImplementation(arguments.element).Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(NameList.UniqueNames));
            if (IsStringSet)
                x.Value.SS.Should().OnlyHaveUniqueItems();
            else
                x.Value.NS.Should().OnlyHaveUniqueItems();
        });
    }
}