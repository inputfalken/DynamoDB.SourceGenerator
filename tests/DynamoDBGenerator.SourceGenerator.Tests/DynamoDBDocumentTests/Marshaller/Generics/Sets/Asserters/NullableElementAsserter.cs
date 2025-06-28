namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

public abstract class NullableElementAsserter<TSet, TElement> : SetAsserter<TSet, TElement?>
    where TSet : IEnumerable<TElement?> where TElement : class
{
    protected NullableElementAsserter(IEnumerable<TElement?> seed, Func<IEnumerable<TElement?>, TSet> fn) : base(seed,
        fn)
    {
    }

    protected static IEnumerable<string> Strings()
    {
        yield return "John";
        yield return "Tom";
        yield return "Michael";
    }

    [Fact]
    public void Marshall_NullElement_ShouldNotThrow()
    {
        Arguments().Should().AllSatisfy(x =>
        {
            var items = x.element.Element.Append(null).ToList();
            var args = CreateArguments(items);
            var act = () => MarshallImplementation(args.element);
            act.Should().NotThrow();
        });
    }
}