using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(File))]
public partial class MemoryStreamTests
{
    private static readonly File Dto = new(new MemoryStream());
    private static readonly Dictionary<string, AttributeValue> AttributeValues = new()
    {
        {nameof(File.Stream), new AttributeValue{B = Dto.Stream}}
    };


    [Fact]
    public void Marshall()
    {
        FileMarshaller.Marshall(Dto).Should().BeEquivalentTo(AttributeValues);

    }
    
    [Fact]
    public void UnMarshall()
    {
        FileMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Dto);

    }
    

    public record File(MemoryStream Stream);
}