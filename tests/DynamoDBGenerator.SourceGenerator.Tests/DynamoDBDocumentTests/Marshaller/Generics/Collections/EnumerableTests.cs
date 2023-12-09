using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<IEnumerable<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableEnumerableElementTests : NoneNullableElementAsserter<IEnumerable<string>, string>
{

    public NoneNullableEnumerableElementTests() : base(Strings(), x => x)
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IEnumerable<string>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IEnumerable<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Should_NotBeLoaded()
    {

        var (_, attributeValues) = CreateArguments(new[] {"Hello!"});

        var res = TextMarshaller.Unmarshall(attributeValues);

        res.Rows.TryGetNonEnumeratedCount(out _).Should().BeFalse();
    }
}

[DynamoDBMarshaller(typeof(Text<IEnumerable<string?>>))]
public partial class NullableEnumerableElementTests : NullableElementAsserter<IEnumerable<string?>, string?>
{

    public NullableEnumerableElementTests() : base(Strings(), x => x)
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IEnumerable<string?>> text)
    {

        return TextMarshaller.Marshall(text);
    }
    protected override Text<IEnumerable<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Should_NotBeLoaded()
    {

        var (_, attributeValues) = CreateArguments(new[] {"Hello!"});

        var res = TextMarshaller.Unmarshall(attributeValues);

        res.Rows.TryGetNonEnumeratedCount(out _).Should().BeFalse();
    }
}