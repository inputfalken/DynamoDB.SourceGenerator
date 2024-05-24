using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(EntityType = typeof(GsiHashAndRangeKey))]
public partial class DynamoDBGsiKeyMarshallerTests
{
    [Fact(Skip = "Could be nice to validate this before the marshaller is created.")]
    public void IndexKeyMarshaller_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IndexKeyMarshaller_MatchedIndexName_ShouldHaveIndex()
    {
        GsiHashAndRangeKeyMarshaller
            .IndexKeyMarshaller(GsiHashAndRangeKey.IndexName)
            .Index
            .Should()
            .Be(GsiHashAndRangeKey.IndexName);
    }

    [Fact]
    public void PartitionKey_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown").PartitionKey("test");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RangeKey_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown").RangeKey("test");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Keys_MissMatchedIndexName_ShouldThrow()
    {
        var act = () => GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller("Unknown").Keys("1", 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PartitionKey_MatchedIndexName_ShouldMapCorrectly()
    {
        GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller(GsiHashAndRangeKey.IndexName)
            .PartitionKey("something@domain.com")
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(GsiHashAndRangeKey.Email));
                x.Value.S.Should().Be("something@domain.com");
            });
    }

    [Fact]
    public void RangeKey_MatchedIndexName_ShouldMapCorrectly()
    {
        GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller(GsiHashAndRangeKey.IndexName)
            .RangeKey(1)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(GsiHashAndRangeKey.EmailRanking));
                x.Value.N.Should().Be("1");
            });
    }

    [Fact]
    public void Keys_MatchedIndexName_ShouldMapCorrectly()
    {
        GsiHashAndRangeKeyMarshaller.IndexKeyMarshaller(GsiHashAndRangeKey.IndexName)
            .Keys("something@domain.com", 1)
            .Should()
            .SatisfyRespectively(x =>
                {

                    x.Key.Should().Be(nameof(GsiHashAndRangeKey.Email));
                    x.Value.S.Should().Be("something@domain.com");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(GsiHashAndRangeKey.EmailRanking));
                    x.Value.N.Should().Be("1");

                }
            );
    }
}

public class GsiHashAndRangeKey
{
    public const string IndexName = "EmailGSI";

    [DynamoDBHashKey]
    public string PrimaryPartitionKey { get; set; } = null!;

    [DynamoDBRangeKey]
    public string PrimaryRangeKey { get; set; } = null!;

    [DynamoDBGlobalSecondaryIndexHashKey(IndexName)]
    public string Email { get; set; } = null!;

    [DynamoDBGlobalSecondaryIndexRangeKey(IndexName)]
    public int EmailRanking { get; set; }
}
