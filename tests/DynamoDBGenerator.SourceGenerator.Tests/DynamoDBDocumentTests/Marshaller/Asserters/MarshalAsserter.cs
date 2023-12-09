using System.ComponentModel;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

public abstract class MarshalAsserter<T, TSeed>
{
    private readonly TSeed _seed;

    protected abstract (T element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(TSeed arg);

    protected (T element, Dictionary<string, AttributeValue> attributeValues) Arguments() => CreateArguments(_seed);
    protected abstract T UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues);
    protected abstract Dictionary<string, AttributeValue> MarshallImplementation(T element);


    protected MarshalAsserter(TSeed seed)
    {
        _seed = seed;
    }

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

public abstract class RecordMarshalAsserter<T, TSeed> : MarshalAsserter<Container<T>, TSeed>
{
    private readonly Func<TSeed, AttributeValue> _fn;
    private readonly Func<TSeed, T> _fn2;

    protected override (Container<T> element, Dictionary<string, AttributeValue> attributeValues) CreateArguments(TSeed arg)
    {
        var value = _fn2(arg);
        var container = new Container<T>(value);

        return (container, new Dictionary<string, AttributeValue>
        {
            {nameof(Container<T>.Element), _fn(arg)}
        });

    }
    protected RecordMarshalAsserter(TSeed seed, Func<TSeed, AttributeValue> fn, Func<TSeed, T> fn2) : base(seed)
    {
        _fn = fn;
        _fn2 = fn2;
    }



    [Fact]
    public void Marshall_Null_ShouldThrow()
    {
        var act = () => MarshallImplementation(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
    public record Container<T>(T Element);
