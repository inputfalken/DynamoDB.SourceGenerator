using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

public abstract class NotNulStringSetAsserter<TSet> : MarshalAsserter<NotNulStringSetAsserter<TSet>.NameList, IEnumerable<string>> where TSet : IEnumerable<string>
{
    private readonly Func<IEnumerable<string>, TSet> _func;

    private static IEnumerable<string> EnumerableImplementation()
    {
        yield return "John";
        yield return "Tom";
        yield return "Michael";
    }

    public record NameList(TSet UniqueNames);

    protected override (NameList element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(IEnumerable<string> arg)
    {
        var text = _func(arg);
        return (new NameList(text), new Dictionary<string, AttributeValue> {{nameof(NameList.UniqueNames), new AttributeValue {SS = text.ToList()}}});
    }


    protected NotNulStringSetAsserter(Func<IEnumerable<string>, TSet> func) : base(func(EnumerableImplementation()))
    {
        _func = func;
    }

    [Fact]
    public void Marshall_NullElement_ShouldThrow()
    {
        var (text, _) = CreateArguments(new[] {"A", "B", null!});
        var act = () => MarshallImplementation(text);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be($"{nameof(NameList.UniqueNames)}[UNKNOWN]");
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

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(NameList.UniqueNames));
    }

    [Fact]
    public void Marshall_NoDuplicated_Elements()
    {
        var (element, _) = CreateArguments(new[] {"John", "John", "Tom"});

        MarshallImplementation(element).Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(NameList.UniqueNames));
            x.Value.SS.Should().SatisfyRespectively(y => y.Should().Be("John"), y => y.Should().Be("Tom"));
        });
    }
}