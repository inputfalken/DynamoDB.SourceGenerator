using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

public abstract class NoneNullableCollectionElementAsserter<TCollection> : MarshalAsserter<NoneNullableCollectionElementAsserter<TCollection>.Text, IEnumerable<string>> where TCollection : IEnumerable<string>
{
    private readonly Func<IEnumerable<string>, TCollection> _func;

    private static IEnumerable<string> EnumerableImplementation()
    {
        yield return "foo";
        yield return "bar";
        yield return "john";
        yield return "doe";
        yield return "";
    }

    public record Text(TCollection Rows);

    protected override (Text element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(IEnumerable<string> arg)
    {
        var text = _func(arg);
        return (
            new Text(text),
            new()
            {
                {
                    nameof(Text.Rows), new AttributeValue {L = text.Select(x => new AttributeValue {S = x}).ToList()}
                }
            }
        );
    }


    protected NoneNullableCollectionElementAsserter(Func<IEnumerable<string>, TCollection> func) : base(func(EnumerableImplementation()))
    {
        _func = func;
    }


    [Fact]
    public void Marshall_NullElement_ShouldThrow()
    {
        var (text, _) = CreateArguments(new[] {"A", "B", null!});
        var act = () => MarshallImplementation(text);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be($"{nameof(Text.Rows)}[2]");
    }

    [Fact]
    public void Marshall_Empty()
    {
        var (text, attributeValues) = CreateArguments(Enumerable.Empty<string>());
        MarshallImplementation(text).Should().BeEquivalentTo(attributeValues);
    }

    [Fact]
    public void Unmarshall_Empty()
    {
        var (text, attributeValues) = CreateArguments(Enumerable.Empty<string>());
        UnmarshallImplementation(attributeValues).Should().BeEquivalentTo(text);
    }

    [Fact]
    public void Unmarshall_EmptyAttributeValues_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(new Dictionary<string, AttributeValue>());

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Text.Rows));
    }
}