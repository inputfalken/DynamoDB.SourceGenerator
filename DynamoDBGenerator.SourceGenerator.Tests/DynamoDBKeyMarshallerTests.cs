using AutoFixture;
namespace DynamoDBGenerator.SourceGenerator.Tests;

[DynamoDBMarshaller(typeof(TypeWithPartitionKeyOnly), PropertyName = "PartitionKeyOnly")]
public partial class DynamoDBKeyMarshallerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void PartitionKey_TypeWithPartitionKeyOnly_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithPartitionKeyOnly>();
        PartitionKeyOnly
            .PartitionKey(type.Id)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(TypeWithPartitionKeyOnly.Id));
                x.Value.S.Should().Be(type.Id);
            });

    }

    [Fact]
    public void RangeKey_TypeWithPartitionKeyOnly_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithPartitionKeyOnly>();
        var act = () => PartitionKeyOnly.RangeKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithPartitionKeyOnly_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => PartitionKeyOnly.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }


}

public class TypeWithPartitionKeyOnly
{
    [DynamoDBHashKey]
    public string Id { get; set; }
}

public class TypeWithRangeKey
{
    [DynamoDBHashKey]
    public string Id { get; set; }
}

public class TypeWithKeys
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBRangeKey]
    public string RangeKey { get; set; }
}

public class TypeWithoutKeys
{

    public string Id { get; set; }

}