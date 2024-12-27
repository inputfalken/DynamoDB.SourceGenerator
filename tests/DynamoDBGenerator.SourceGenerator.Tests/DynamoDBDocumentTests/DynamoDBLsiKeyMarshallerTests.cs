using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(EntityType = typeof(LsiHashAndRangeKey))]
public partial class DynamoDBLsiKeyMarshallerTests
{
    [Fact(Skip = "Could be nice to validate this before the marshaller is created.")]
    public void IndexKeyMarshaller_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IndexKeyMarshaller_MatchedIndexName_ShouldHaveIndex()
    {
        LsiHashAndRangeKeyMarshaller
            .IndexKeyMarshaller(LsiHashAndRangeKey.IndexName)
            .Index
            .Should()
            .Be(LsiHashAndRangeKey.IndexName);
    }

    [Fact]
    public void PartitionKey_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown").PartitionKey("test");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RangeKey_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown").RangeKey("test");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Keys_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown").Keys("1", 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PartitionKey_MatchedIndexName_ShouldMapCorrectly()
    {
        LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller(LsiHashAndRangeKey.IndexName)
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
        LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller(LsiHashAndRangeKey.IndexName)
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
        LsiHashAndRangeKeyMarshaller.IndexKeyMarshaller(LsiHashAndRangeKey.IndexName)
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
    public string Email { get; set; } = null!;

    [DynamoDBRangeKey]
    public string PrimaryRangeKey { get; set; } = null!;


    [DynamoDBLocalSecondaryIndexRangeKey(IndexName)]
    public int EmailRanking { get; set; }
}