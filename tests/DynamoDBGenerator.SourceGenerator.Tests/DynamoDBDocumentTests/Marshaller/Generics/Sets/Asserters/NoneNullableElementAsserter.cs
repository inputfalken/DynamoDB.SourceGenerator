using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

public abstract class NoneNullableElementAsserter<TSet, TElement>(
    IEnumerable<TElement> seed,
    Func<IEnumerable<TElement>, TSet> fn
) : SetAsserter<TSet, TElement>(seed, fn)
    where TSet : IEnumerable<TElement>
    where TElement : class
{
    protected static IEnumerable<string> Strings()
    {
        yield return "John";
        yield return "Tom";
        yield return "Michael";
    }

    [Fact]
    public void Marshall_NullElement_ShouldThrow()
    {
        Arguments().Should().AllSatisfy(x =>
        {
            var items = x.element.Element.Append(null).ToList();
            var args = CreateArguments(items!);
            var act = () => MarshallImplementation(args.element);
            act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should()
                .Be($"{nameof(Container<TSet>.Element)}[UNKNOWN]");
        });
    }
}