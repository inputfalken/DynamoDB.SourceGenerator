namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class KeyValuePairTests
{
    [Fact]
    public void BuildAttributeValues_KeyAndValueSet_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string?, int?>("1", 2)
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x =>
            {
                x.Key.Should().Be(nameof(KeyValuePairClass.KeyValuePair));
                x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be("Key");
                        y.Value.S.Should().Be("1");
                    },
                    y =>
                    {
                        y.Key.Should().Be("Value");
                        y.Value.N.Should().Be("2");
                    }
                );
            }
        );
    }

    [Fact]
    public void BuildAttributeValues_NullKey_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string?, int?>(null, 2)
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x =>
            {
                x.Key.Should().Be(nameof(KeyValuePairClass.KeyValuePair));
                x.Value.M.Should().SatisfyRespectively(
                    y =>
                    {
                        y.Key.Should().Be("Value");
                        y.Value.N.Should().Be("2");
                    }
                );
            }
        );
    }

    [Fact]
    public void BuildAttributeValues_NullValue_Included()
    {
        var @class = new KeyValuePairClass()
        {
            KeyValuePair = new KeyValuePair<string?, int?>("abc", null)
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x =>
            {
                x.Key.Should().Be(nameof(KeyValuePairClass.KeyValuePair));
                x.Value.M.Should().SatisfyRespectively(
                    y =>
                    {
                        y.Key.Should().Be("Key");
                        y.Value.S.Should().Be("abc");
                    }
                );
            }
        );
    }
}

[AttributeValueGenerator]
public partial class KeyValuePairClass
{
    [DynamoDBProperty]
    public KeyValuePair<string?, int?> KeyValuePair { get; set; }
}