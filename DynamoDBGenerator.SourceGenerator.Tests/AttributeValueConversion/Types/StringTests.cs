namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Types;

public class StringTests
{
    [Fact]
    public void BuildAttributeValues_StringProperty_Included()
    {
        const string johnDoe = "John Doe";
        var @class = new StringClass
        {
            Name = johnDoe
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(StringClass.Name))
            .And
            .ContainSingle(x => x.Value.S == johnDoe);
    }
}

[AttributeValueGenerator]
public partial class StringClass
{
    [DynamoDBProperty]
    public string? Name { get; set; }
}