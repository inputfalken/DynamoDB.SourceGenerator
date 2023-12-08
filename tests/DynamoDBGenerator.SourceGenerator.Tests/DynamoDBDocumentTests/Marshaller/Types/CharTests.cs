using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container))]
public partial class CharTests : RecordMarshalAsserter<char, char>
{


    [Fact(Skip = "TODO")]
    public void Unmarshall_EmptyString_ShouldThrow()
    {
        var act = () => ContainerMarshaller.Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Container.Element), new AttributeValue {S = ""}}});
        act.Should().Throw<DynamoDBMarshallingException>();
    }

    protected override Container UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container element)
    {
        return ContainerMarshaller.Marshall(element);
    }


    public CharTests() : base('A', x => new AttributeValue {S = x.ToString()}, c => c)
    {
    }
}