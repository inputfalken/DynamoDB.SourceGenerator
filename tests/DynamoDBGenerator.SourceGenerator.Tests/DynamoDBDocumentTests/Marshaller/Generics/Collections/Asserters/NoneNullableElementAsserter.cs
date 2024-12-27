using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

public abstract class NoneNullableElementAsserter<TCollection, TElement> : CollectionAsserter<TCollection, TElement>
    where TCollection : IEnumerable<TElement> where TElement : class
{
    protected NoneNullableElementAsserter(IEnumerable<TElement> seed, Func<IEnumerable<TElement>, TCollection> func) :
        base(seed, x => new AttributeValue { S = x?.ToString() }, func)
    {
    }

    protected static IEnumerable<string> Strings()
    {
        yield return "foo";
        yield return "bar";
        yield return "john";
        yield return "doe";
        yield return "";
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
                .Be($"{nameof(Container<TCollection>.Element)}[{items.IndexOf(null)}]");
        });
    }
}