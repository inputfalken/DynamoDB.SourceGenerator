namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Generics;

[DynamoDBGenerator.DynamoDBDocument(typeof(KeyValuePairClass))]
public partial class KeyValuePairTests
{
    [Fact]
    public void Serialize_KeyAndValueSet_Included()
    {
        var @class = new KeyValuePairClass
        {
            KeyValuePair = new KeyValuePair<string?, int?>("1", 2)
        };

        KeyValuePairClassDocument
            .Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(KeyValuePairClass.KeyValuePair));
                    x.Value.M.Should().SatisfyRespectively(y =>
                        {
                            ((string)y.Key).Should().Be("Key");
                            ((string)y.Value.S).Should().Be("1");
                        },
                        y =>
                        {
                            ((string)y.Key).Should().Be("Value");
                            ((string)y.Value.N).Should().Be("2");
                        }
                    );
                }
            );
    }

    [Fact]
    public void Serialize_NullKey_Included()
    {
        var @class = new KeyValuePairClass
        {
            KeyValuePair = new KeyValuePair<string?, int?>(null, 2)
        };

        KeyValuePairClassDocument
            .Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(KeyValuePairClass.KeyValuePair));
                    x.Value.M.Should().SatisfyRespectively(
                        y =>
                        {
                            ((string)y.Key).Should().Be("Value");
                            ((string)y.Value.N).Should().Be("2");
                        }
                    );
                }
            );
    }

    [Fact]
    public void Serialize_NullValue_Included()
    {
        var @class = new KeyValuePairClass
        {
            KeyValuePair = new KeyValuePair<string?, int?>("abc", null)
        };

        KeyValuePairClassDocument
            .Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    ((string)x.Key).Should().Be(nameof(KeyValuePairClass.KeyValuePair));
                    x.Value.M.Should().SatisfyRespectively(
                        y =>
                        {
                            ((string)y.Key).Should().Be("Key");
                            ((string)y.Value.S).Should().Be("abc");
                        }
                    );
                }
            );
    }
}

public class KeyValuePairClass
{
    [DynamoDBProperty]
    public KeyValuePair<string?, int?> KeyValuePair { get; set; }
}
