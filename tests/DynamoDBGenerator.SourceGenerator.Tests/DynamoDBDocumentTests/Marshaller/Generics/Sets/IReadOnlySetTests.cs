using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(NameList))]
public partial class IReadOnlySetTests : NotNulStringSetAsserter<IReadOnlySet<string>>
{
    public IReadOnlySetTests() : base(x => new SortedSet<string>(x))
    {

    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(NameList element)
    {
        return NameListMarshaller.Marshall(element);
    }

    protected override NameList UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return NameListMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeHashset()
    {
        var (_, attributeValues) = DefaultArguments;

        NameListMarshaller.Unmarshall(attributeValues).UniqueNames.Should().BeOfType<HashSet<string>>();
    }
}