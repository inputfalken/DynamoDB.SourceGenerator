using System.Text;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBMarshaller(typeof(WithStream))]
public partial class StreamTests
{

    [Fact]
    public void Unmarshall_Support_Stream()
    {
        var subject = WithStreamMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {

                {
                    nameof(WithStream.MyMemoryStream), new AttributeValue
                    {
                        B = new MemoryStream(Encoding.UTF8.GetBytes("Hello"))
                    }
                }
            })
            .Should()
            .BeOfType<WithStream>()
            .Subject;

        using var streamWriter = new StreamReader(subject.MyMemoryStream);

        streamWriter.ReadToEnd().Should().Be("Hello");
    }
}

public class WithStream
{
    public MemoryStream MyMemoryStream { get; set; }
}