using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text))]
// ReSharper disable once UnusedType.Global
public partial class EnumerableTests : NoneNullableElementAsserter<IEnumerable<string>, string>
{

    public EnumerableTests() : base(Strings(), x => x)
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Should_NotBelLoaded()
    {

        var (_, attributeValues) = CreateArguments(new[] {"Hello!"});

        var res = TextMarshaller.Unmarshall(attributeValues);

        res.Rows.TryGetNonEnumeratedCount(out _).Should().BeFalse();
    }
}