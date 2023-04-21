namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class KeyValuePairTests
{
    [Fact]
    public void BuildAttributeValues_KeyAndValueSet_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string, int?>("1", 2)
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

    [Fact]
    public void BuildAttributeValues_NullKey_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string, int?>(null, 2)
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_NullValue_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string, int?>("abc", null)
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }
}

[AttributeValueGenerator]
public partial class KeyValuePairClass
{
    [DynamoDBProperty]
    public KeyValuePair<string, int?> KeyValuePair { get; set; }
}