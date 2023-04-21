namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class KeyValuePairTests
{
    [Fact]
    public void BuildAttributeValues_KeyValueProperty_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string, int>("1", 2)
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(KeyValuePairClass.KeyValuePair))
            .And
            .SatisfyRespectively(x =>
            {
                x.Value.M.Should().SatisfyRespectively(y =>
                {
                    y.Key.Should().Be("1");
                    y.Value.N.Should().Be("2");
                });
            });
    }
}

[AttributeValueGenerator]
public partial class KeyValuePairClass
{
    [DynamoDBProperty]
    public KeyValuePair<string, int> KeyValuePair { get; set; }
}