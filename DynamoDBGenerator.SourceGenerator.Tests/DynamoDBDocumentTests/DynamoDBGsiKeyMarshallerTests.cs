using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(typeof(GsiHashKeyOnly))]
public partial class DynamoDBGsiKeyMarshallerTests
{

    [Fact]
    public void UnmatchedIndexName_Throws()
    {
        var indexedKeyMarshaller = GsiHashKeyOnlyMarshallerWithIndex("I DO NOT EXIST");

        const string exceptionMessage = "Could not find any index match for value 'I DO NOT EXIST'. (Parameter 'index')";
        var indexedRangeKeyAct = () => indexedKeyMarshaller.RangeKey("Id");
        var indexedPartitionAct = () => indexedKeyMarshaller.PartitionKey("ID");
        var indexedKeysAct = () => indexedKeyMarshaller.Keys("id1", "id2");
        indexedRangeKeyAct.Should().Throw<ArgumentOutOfRangeException>().WithMessage(exceptionMessage);
        indexedPartitionAct.Should().Throw<ArgumentOutOfRangeException>().WithMessage(exceptionMessage);
        indexedKeysAct.Should().Throw<ArgumentOutOfRangeException>().WithMessage(exceptionMessage);

        // Verify that Primary access still would work.
        var primaryKeyMarshaller = GsiHashKeyOnlyMarshaller;
        var rangeKeyAct = () => primaryKeyMarshaller.RangeKey("Id");
        var partitionAct = () => primaryKeyMarshaller.PartitionKey("ID");
        var keysAct = () => primaryKeyMarshaller.Keys("id1", "id2");
        rangeKeyAct.Should().NotThrow();
        partitionAct.Should().NotThrow();
        keysAct.Should().NotThrow();
    }

    [Fact]
    public void MatchedIndexName_Should_MapCorrectly()
    {
        var indexedKeyMarshaller = GsiHashKeyOnlyMarshallerWithIndex(GsiHashKeyOnly.IndexName);

        var result = indexedKeyMarshaller.PartitionKey("something@domain.com");

        result.Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(GsiHashKeyOnly.Email));
            x.Value.S.Should().Be("something@domain.com");
        });
    }

}

public class GsiHashKeyOnly
{
    public const string IndexName = "EmailGSI";

    [DynamoDBHashKey]
    public string PrimaryPartitionKey { get; set; }

    [DynamoDBRangeKey]
    public string PrimaryRangeKey { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey(IndexName)]
    public string Email { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey(IndexName)]
    public int EmailRanking { get; set; }
}