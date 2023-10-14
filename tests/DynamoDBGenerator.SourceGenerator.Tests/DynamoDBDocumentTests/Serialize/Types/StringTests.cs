using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBMarshaller(typeof(StringClass))]
public partial class StringTests
{
    [Fact]
    public void Serialize_StringProperty_Included()
    {
        const string johnDoe = "John Doe";
        var @class = new StringClass
        {
            Name = johnDoe
        };

        StringClassMarshaller
            .Marshall(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(StringClass.Name))
            .And
            .ContainSingle(x => x.Value.S == johnDoe);
    }
}

public class StringClass
{
    [DynamoDBProperty]
    public string? Name { get; set; }
}
