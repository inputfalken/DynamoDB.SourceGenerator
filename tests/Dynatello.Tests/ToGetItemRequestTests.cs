using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Exceptions;
using Dynatello.Builders;
using FluentAssertions;

namespace Dynatello.Tests;

public class ToGetItemRequestTests
{
    private static readonly GetItemRequestBuilder<Guid> GetCatByPartitionKey;
    private static readonly GetItemRequestBuilder<(Guid partionKey, Guid rangeKey)> GetCatByCompositeKeys;

    static ToGetItemRequestTests()
    {
        GetCatByPartitionKey = Cat.QueryWithCuteness.OnTable("TABLE").ToGetRequestBuilder(x => x.Id);
        GetCatByCompositeKeys = Cat.QueryWithCuteness.OnTable("TABLE").ToGetRequestBuilder(x => x.Id, x => x.HomeId);
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidPartition()
    {
        var act = () => Cat.QueryWithCuteness
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Name, x => x.HomeId)
            .Build(("", Guid.Empty));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidRange()
    {
        var act = () => Cat.QueryWithCuteness
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Id, x => x.Name)
            .Build((Guid.Empty, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidPartitionAndRange()
    {
        var act = () => Cat.QueryWithCuteness
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Cuteness, x => x.Name)
            .Build((2.3, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_WithInvalidPartitionKey()
    {
        var act = () => Cat.QueryWithCuteness
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Name)
            .Build("TEST");
        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_PartitionKeyOnly()
    {
        Cat.Fixture.CreateMany<Guid>().Should().AllSatisfy(partitionKey =>
        {
            var request = GetCatByPartitionKey.Build(partitionKey);

            request.Key
                .Should()
                .BeEquivalentTo(new Dictionary<string, AttributeValue>()
                    { { nameof(Cat.Id), new AttributeValue { S = partitionKey.ToString() } } }
                );

            request.TableName.Should().Be("TABLE");
        });
    }

    [Fact]
    public void Build_Request_CompositeKeys()
    {
        Cat.Fixture.CreateMany<(Guid PartitionKey, Guid RangeKey)>().Should().AllSatisfy(keys =>
        {
            var request = GetCatByCompositeKeys.Build(keys);

            request.Key
                .Should()
                .BeEquivalentTo(new Dictionary<string, AttributeValue>()
                    {
                        { nameof(Cat.Id), new AttributeValue { S = keys.PartitionKey.ToString() } },
                        { nameof(Cat.HomeId), new AttributeValue { S = keys.RangeKey.ToString() } }
                    }
                );

            request.TableName.Should().Be("TABLE");
        });
    }
}