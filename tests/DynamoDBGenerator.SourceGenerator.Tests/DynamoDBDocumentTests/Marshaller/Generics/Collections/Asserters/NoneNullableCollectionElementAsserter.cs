using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

public abstract class NoneNullableCollectionElementAsserter<TCollection> : CollectionAsserter<TCollection, string> where TCollection : IEnumerable<string>
{

    private static IEnumerable<string> EnumerableImplementation()
    {
        yield return "foo";
        yield return "bar";
        yield return "john";
        yield return "doe";
        yield return "";
    }


    protected NoneNullableCollectionElementAsserter(Func<IEnumerable<string>, TCollection> func) : base(EnumerableImplementation(), func)
    {
    }


    [Fact]
    public void Marshall_NullElement_ShouldThrow()
    {
        var (text, _) = CreateArguments(new[] {"A", "B", null!});
        var act = () => MarshallImplementation(text);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be($"{nameof(Text.Rows)}[2]");
    }

}