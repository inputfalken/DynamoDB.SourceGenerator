using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Person))]
public partial class StringTests
{
    private static readonly Person Dto = new("John");

    private static readonly Dictionary<string, AttributeValue> AttributeValues = new()
    {
        {nameof(Person.Name), new AttributeValue {S = Dto.Name}}
    };


    [Fact]
    public void Marshall()
    {
        PersonMarshaller.Marshall(Dto).Should().BeEquivalentTo(AttributeValues);
    }
    
    [Fact]
    public void Unmarshall()
    {
        PersonMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Dto);
    }


    public record Person(string Name);
}