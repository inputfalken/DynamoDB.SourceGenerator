using System.Text;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBMarshaller(typeof(WithStream))]
public partial class StreamTests
{
    [Fact]
    public void Marshall_Support_Stream()
    {
        var subject = WithStreamMarshaller
            .Marshall(new WithStream {MyMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello"))})
            .Should()
            .BeOfType<Dictionary<string, AttributeValue>>()
            .Subject;

        using var streamWriter = new StreamReader(subject[nameof(WithStream.MyMemoryStream)].B);
        streamWriter.ReadToEnd().Should().Be("Hello");
    }

}

public class WithStream
{
    public MemoryStream MyMemoryStream { get; set; } = null!;
}