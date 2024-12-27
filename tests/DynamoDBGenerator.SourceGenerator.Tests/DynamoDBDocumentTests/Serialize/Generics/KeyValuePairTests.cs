using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

// TODO these tests are giving warnings due to the return type of Unmarshall does not take nullability into account.
// This could maybe be solved by digging down into generic the types in order to determine whether they should be nullable and then build the return type based on that information.
// This behaviour is already solved for methods that should follow the nullability of data members.
[DynamoDBMarshaller(EntityType = typeof(KeyValuePairClass))]
public partial class KeyValuePairTests
{
    [Fact]
    public void Serialize_KeyAndValueSet_Included()
    {
        var @class = new KeyValuePairClass
        {
            KeyValuePair = new KeyValuePair<string?, int?>("1", 2)
        };

        KeyValuePairClassMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(KeyValuePairClass.KeyValuePair));
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

        KeyValuePairClassMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(KeyValuePairClass.KeyValuePair));
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

        KeyValuePairClassMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(KeyValuePairClass.KeyValuePair));
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