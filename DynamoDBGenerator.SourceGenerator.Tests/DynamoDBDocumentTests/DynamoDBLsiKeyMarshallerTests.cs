using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(typeof(LsiHashAndRangeKey))]
public partial class DynamoDBLsiKeyMarshallerTests
{
    [Fact]
    public void PartitionKey_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshallerWithIndex("Unknown").PartitionKey("test");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RangeKey_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshallerWithIndex("Unknown").RangeKey("test");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Keys_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshallerWithIndex("Unknown").Keys("1", 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PartitionKey_MatchedIndexName_ShouldMapCorrectly()
    {
        LsiHashAndRangeKeyMarshallerWithIndex(LsiHashAndRangeKey.IndexName)
            .PartitionKey("something@domain.com")
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(LsiHashAndRangeKey.Email));
                x.Value.S.Should().Be("something@domain.com");
            });
    }

    [Fact]
    public void RangeKey_MatchedIndexName_ShouldMapCorrectly()
    {
        LsiHashAndRangeKeyMarshallerWithIndex(LsiHashAndRangeKey.IndexName)
            .RangeKey(1)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(LsiHashAndRangeKey.EmailRanking));
                x.Value.N.Should().Be("1");
            });
    }

    [Fact]
    public void Keys_MatchedIndexName_ShouldMapCorrectly()
    {
        LsiHashAndRangeKeyMarshallerWithIndex(LsiHashAndRangeKey.IndexName)
            .Keys("something@domain.com", 1)
            .Should()
            .SatisfyRespectively(x =>
                {

                    x.Key.Should().Be(nameof(LsiHashAndRangeKey.Email));
                    x.Value.S.Should().Be("something@domain.com");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(LsiHashAndRangeKey.EmailRanking));
                    x.Value.N.Should().Be("1");

                }
            );
    }

}

public class LsiHashAndRangeKey
{
    public const string IndexName = "EmailLSI";


    [DynamoDBHashKey]
    public string Email { get; set; }

    [DynamoDBRangeKey]
    public string PrimaryRangeKey { get; set; }


    [DynamoDBLocalSecondaryIndexRangeKey(IndexName)]
    public int EmailRanking { get; set; }
}