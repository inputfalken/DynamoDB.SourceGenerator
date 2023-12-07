using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text))]
// ReSharper disable once UnusedType.Global
public partial class EnumerableTests : NoneNullableCollectionElementAsserter<IEnumerable<string>>
{

    public EnumerableTests() : base(x => x)
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