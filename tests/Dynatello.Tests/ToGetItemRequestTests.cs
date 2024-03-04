using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Exceptions;
using Dynatello.Builders;
using FluentAssertions;

namespace Dynatello.Tests;

public class ToGetItemRequestTests
{
    [Fact]
    public void Build_Request_CompositeKeys_InvalidPartition()
    {
        var act = () => Cat.GetByCompositeInvalidPartition
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Id, x => x.HomeId)
            .Build(("", Guid.Empty));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidRange()
    {
        var act = () => Cat.GetByCompositeInvalidRange
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Id, x => x.HomeId)
            .Build((Guid.Empty, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidPartitionAndRange()
    {
        var act = () => Cat.GetByCompositeInvalidPartitionAndRange
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Id, x => x.HomeId)
            .Build((2.3, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_WithInvalidPartitionKey()
    {
        var act = () => Cat.GetByInvalidPartition
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x)
            .Build("TEST");

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_PartitionKeyOnly()
    {
        var getCatByPartitionKey = Cat.GetById
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x);

        Cat.Fixture
            .CreateMany<Guid>()
            .Should()
            .AllSatisfy(x => getCatByPartitionKey
                .Build(x)
                .Should()
                .BeEquivalentTo(new GetItemRequest
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { nameof(Cat.Id), new AttributeValue { S = x.ToString() } }
                    },
                    TableName = "TABLE",
                    ConsistentRead = false,
                    ExpressionAttributeNames = new Dictionary<string, string>(),
                    ProjectionExpression = null,
                    ReturnConsumedCapacity = null,
                    AttributesToGet = new List<string>()
                }));
    }

    [Fact]
    public void Build_Request_CompositeKeys()
    {
        var getCatByCompositeKeys = Cat.GetByCompositeKey
            .OnTable("TABLE")
            .ToGetRequestBuilder(x => x.Id, x => x.HomeId);

        Cat.Fixture
            .CreateMany<(Guid PartitionKey, Guid RangeKey)>()
            .Should()
            .AllSatisfy(x => getCatByCompositeKeys
                .Build(x)
                .Should()
                .BeEquivalentTo(new GetItemRequest
                    {
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { nameof(Cat.Id), new AttributeValue { S = x.PartitionKey.ToString() } },
                            { nameof(Cat.HomeId), new AttributeValue { S = x.RangeKey.ToString() } }
                        },
                        TableName = "TABLE",
                        ConsistentRead = false,
                        ExpressionAttributeNames = new Dictionary<string, string>(),
                        ProjectionExpression = null,
                        ReturnConsumedCapacity = null,
                        AttributesToGet = new List<string>()
                    }
                )
            );
    }
}