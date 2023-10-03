using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(typeof(ReverseAccessIndexSetup))]
public partial class DynamoDBIndexTests
{

    [Fact]
    public void PartitionKey_Primary_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller.PrimaryKeyMarshaller
            .PartitionKey("I AM PRIMARY HASH KEY")
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryHashAndGsiRange));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                }
            );
    }

    [Fact]
    public void PartitionKey_GSI_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.ReversedAccessPatternGsi)
            .PartitionKey("I AM PRIMARY HASH KEY")
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryRangeKeyAndGsiHash));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                }
            );
    }

    [Fact]
    public void PartitionKey_LSI_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.Lsi)
            .PartitionKey("I AM PRIMARY HASH KEY")
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryHashAndGsiRange));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                }
            );
    }


    [Fact]
    public void RangeKey_Primary_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller.PrimaryKeyMarshaller
            .RangeKey("I AM PRIMARY RANGE KEY")
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryRangeKeyAndGsiHash));
                    x.Value.S.Should().Be("I AM PRIMARY RANGE KEY");
                }
            );
    }

    [Fact]
    public void RangeKey_GSI_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.ReversedAccessPatternGsi)
            .RangeKey("I AM PRIMARY RANGE KEY")
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryHashAndGsiRange));
                    x.Value.S.Should().Be("I AM PRIMARY RANGE KEY");
                }
            );
    }

    [Fact]
    public void RangeKey_LSI_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.Lsi)
            .RangeKey("I AM LSI RANGE KEY")
            .Should()
            .SatisfyRespectively(
                x =>
                {

                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.LsiRangeKeyAndGSiHash));
                    x.Value.S.Should().Be("I AM LSI RANGE KEY");
                }
            );
    }

    [Fact]
    public void Keys_Primary_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller.PrimaryKeyMarshaller
            .Keys("I AM PRIMARY HASH KEY", "I AM PRIMARY RANGE KEY")
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryHashAndGsiRange));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryRangeKeyAndGsiHash));
                    x.Value.S.Should().Be("I AM PRIMARY RANGE KEY");
                }
            );
    }

    [Fact]
    public void Keys_GSI_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.ReversedAccessPatternGsi)
            .Keys("I AM PRIMARY HASH KEY", "I AM PRIMARY RANGE KEY")
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryRangeKeyAndGsiHash));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryHashAndGsiRange));
                    x.Value.S.Should().Be("I AM PRIMARY RANGE KEY");
                }
            );
    }

    [Fact]
    public void Keys_LSI_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.Lsi)
            .Keys("I AM PRIMARY HASH KEY", "I AM LSI RANGE KEY")
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.PrimaryHashAndGsiRange));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                },
                x =>
                {

                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.LsiRangeKeyAndGSiHash));
                    x.Value.S.Should().Be("I AM LSI RANGE KEY");
                }
            );
    }

    [Fact]
    public void PartitionKey_GsiPartitionKeyOnly_KeyAccess()
    {
        ReverseAccessIndexSetupMarshaller
            .IndexKeyMarshaller(ReverseAccessIndexSetup.PartitionGsiOnly)
            .PartitionKey("I AM PRIMARY HASH KEY")
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be(nameof(ReverseAccessIndexSetup.LsiRangeKeyAndGSiHash));
                    x.Value.S.Should().Be("I AM PRIMARY HASH KEY");
                }
            );
    }

}

public class ReverseAccessIndexSetup
{
    public const string ReversedAccessPatternGsi = "REVERSED_ACCESS_PATTERN";
    public const string PartitionGsiOnly = "SINGLE_GSI";

    public const string Lsi = "LSI";

    // Is used to ensure we don't mix indexes
    private const string PrivateLsi = "1";

    // Is used to ensure we don't mix indexes
    private const string PrivateLsi2 = "2";

    [DynamoDBHashKey]
    [DynamoDBGlobalSecondaryIndexRangeKey(ReversedAccessPatternGsi)]
    [DynamoDBLocalSecondaryIndexRangeKey(PrivateLsi)]

    public string? PrimaryHashAndGsiRange { get; set; }

    [DynamoDBRangeKey]
    [DynamoDBGlobalSecondaryIndexHashKey(ReversedAccessPatternGsi)]
    [DynamoDBLocalSecondaryIndexRangeKey(PrivateLsi2)]
    public string? PrimaryRangeKeyAndGsiHash { get; set; }


    [DynamoDBLocalSecondaryIndexRangeKey(Lsi)]
    [DynamoDBGlobalSecondaryIndexHashKey(PartitionGsiOnly)]
    public string? LsiRangeKeyAndGSiHash { get; set; }


}