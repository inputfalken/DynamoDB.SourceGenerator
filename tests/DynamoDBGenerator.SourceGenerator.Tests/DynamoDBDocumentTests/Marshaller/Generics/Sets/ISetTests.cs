using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(NameList))]
public partial class ISetTests : NotNulStringSetAsserter<ISet<string>>
{
    public ISetTests() : base(x => new HashSet<string>(x))
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