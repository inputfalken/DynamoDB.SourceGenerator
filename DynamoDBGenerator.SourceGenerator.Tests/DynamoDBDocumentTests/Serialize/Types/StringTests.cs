namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBMarshallert(typeof(StringClass))]
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

        StringClassDocument
            .Serialize(@class)
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
