using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBMarshaller(typeof(StringClass))]
public partial class StringTests
{
    [Fact]
    public void Serialize_StringProperty_Included()
    {
        StringClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>()
            {
                {nameof(StringClass.Name), new AttributeValue {S = "John Doe"}}
            })
            .Should()
            .BeOfType<StringClass>()
            .Which
            .Name
            .Should()
            .Be("John Doe");
    }
}

public class StringClass
{
    [DynamoDBProperty]
    public string? Name { get; set; }
}