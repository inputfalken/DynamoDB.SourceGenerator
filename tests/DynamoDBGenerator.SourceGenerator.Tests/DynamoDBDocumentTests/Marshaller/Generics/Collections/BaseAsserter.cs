using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

public abstract class BaseAsserter<TCollection> where TCollection : IEnumerable<string>
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

    protected (Text Text, Dictionary<string, AttributeValue>) ConstructArguments(IEnumerable<string> elements)
    {
        var text = _func(elements);
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

    protected BaseAsserter(Func<IEnumerable<string>, TCollection> func)
    {
        _func = func;
    }

    protected abstract Dictionary<string, AttributeValue> MarshallImplementation(Text text);
    protected abstract Text UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues);

    [Fact]
    public void Marshall()
    {
        var (text, attributeValues) = ConstructArguments(EnumerableImplementation());
        MarshallImplementation(text).Should().BeEquivalentTo(attributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        var (text, attributeValues) = ConstructArguments(EnumerableImplementation());
        UnmarshallImplementation(attributeValues).Should().BeEquivalentTo(text);
    }

    [Fact]
    public void Marshall_NullElement_ShouldThrow()
    {
        var (text, _) = ConstructArguments(new[] {"A", "B", null!});
        var act = () => MarshallImplementation(text);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be($"{nameof(Text.Rows)}[2]");
    }

    [Fact]
    public void Marshall_Empty()
    {
        var (text, attributeValues) = ConstructArguments(Enumerable.Empty<string>());
        MarshallImplementation(text).Should().BeEquivalentTo(attributeValues);
    }

    [Fact]
    public void Unmarshall_Empty()
    {
        var (text, attributeValues) = ConstructArguments(Enumerable.Empty<string>());
        UnmarshallImplementation(attributeValues).Should().BeEquivalentTo(text);
    }

    [Fact]
    public void Unmarshall_EmptyAttributeValues_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(new Dictionary<string, AttributeValue>());

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Text.Rows));
    }
}