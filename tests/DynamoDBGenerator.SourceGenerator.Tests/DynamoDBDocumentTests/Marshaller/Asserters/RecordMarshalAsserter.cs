using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

public abstract class RecordMarshalAsserter<T> : MarshalAsserter<Container<T>>
{
    private readonly IEnumerable<T> _arguments;
    private readonly Func<T, AttributeValue?> _fn;

    protected override IEnumerable<(Container<T> element, Dictionary<string, AttributeValue> attributeValues)> Arguments()
    {
        foreach (var argument in _arguments)
        {

            var container = new Container<T>(argument);

            yield return _fn(argument) is { } attributeValue
                ? (container, new Dictionary<string, AttributeValue> {{nameof(Container<T>.Element), attributeValue}})
                : (container, new Dictionary<string, AttributeValue>());
        }
    }
    protected RecordMarshalAsserter(IEnumerable<T> arguments, Func<T, AttributeValue?> fn)
    {
        _arguments = arguments;
        _fn = fn;
    }


    [Fact]
    public void Marshall_Null_ShouldThrow()
    {
        var act = () => MarshallImplementation(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}

public record Container<T>(T Element);

public abstract class NotNullRecordElementMarshalAsserter<T> : RecordMarshalAsserter<T> where T : class
{

    protected NotNullRecordElementMarshalAsserter(IEnumerable<T> arguments, Func<T, AttributeValue?> fn) : base(arguments, fn)
    {
    }

    [Fact]
    public void Marshall_NullDataMember_ShouldThrow()
    {
        var act = () => MarshallImplementation(new Container<T>(null!));
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Container<T>.Element));
    }

    [Fact]
    public void Unmarshall_NullDataMember_ShouldThrow()
    {
        var act = () => UnmarshallImplementation(new Dictionary<string, AttributeValue>
        {
            {nameof(Container<T>.Element), null!}
        });

        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(nameof(Container<T>.Element));
    }
}