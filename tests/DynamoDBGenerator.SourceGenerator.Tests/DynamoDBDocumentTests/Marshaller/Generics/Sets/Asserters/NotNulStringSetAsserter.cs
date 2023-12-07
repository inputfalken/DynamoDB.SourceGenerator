using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

public abstract class NotNulStringSetAsserter<TSet> : SetAsserter<TSet, string> where TSet : IEnumerable<string>
{

    private static IEnumerable<string> EnumerableImplementation()
    {
        yield return "John";
        yield return "Tom";
        yield return "Michael";
    }


    [Fact]
    public void Marshall_NullElement_ShouldThrow()
    {
        var (text, _) = CreateArguments(new[] {"A", "B", null!});
        var act = () => MarshallImplementation(text);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be($"{nameof(SetDto.Set)}[UNKNOWN]");
    }

    protected NotNulStringSetAsserter(Func<IEnumerable<string>, TSet> fn) : base(EnumerableImplementation(), fn)
    {
    }
}