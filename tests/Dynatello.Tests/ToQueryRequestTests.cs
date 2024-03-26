using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using FluentAssertions;

namespace Dynatello.Tests;

public class ToQueryRequestTests
{
    [Fact]
    public void Build_Request()
    {
        var builder = Cat.QueryWithCuteness
            .OnTable("TABLE")
            .WithKeyConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .ToQueryRequestBuilder();
        Cat.Fixture.CreateMany<(Guid Id, double MinimumCuteness)>(10).Should().AllSatisfy(tuple =>
        {
            builder.Build(tuple).Should().BeEquivalentTo(
                new QueryRequest
                {
                    TableName = "TABLE",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Id", nameof(Cat.Id) }
                    },

                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":p1", new AttributeValue { S = tuple.Id.ToString() } }
                    },
                    KeyConditionExpression = "#Id = :p1",
                    KeyConditions = null,
                    ConditionalOperator = null,
                    AttributesToGet = null,
                    ReturnConsumedCapacity = null,
                    FilterExpression = null,
                    Limit = 0,
                    ConsistentRead = false,
                    Select = null,
                    ProjectionExpression = null,
                    IndexName = null,
                    QueryFilter = null,
                    ExclusiveStartKey = new Dictionary<string, AttributeValue>(),
                    IsLimitSet = false,
                    ScanIndexForward = false
                });
        });
    }

    [Fact]
    public void Build_Request_FilterExpression()
    {
        var builder = Cat.QueryWithCuteness
            .OnTable("TABLE")
            .WithKeyConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .WithFilterExpression((x, y) => $"{x.Cuteness} > {y.MinimumCuteness}")
            .ToQueryRequestBuilder();
        Cat.Fixture.CreateMany<(Guid Id, double MinimumCuteness)>(10).Should().AllSatisfy(tuple =>
        {
            builder.Build(tuple).Should().BeEquivalentTo(
                new QueryRequest
                {
                    TableName = "TABLE",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Id", nameof(Cat.Id) },
                        { "#Cuteness", nameof(Cat.Cuteness) }
                    },

                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":p1", new AttributeValue { S = tuple.Id.ToString() } },
                        {
                            ":p2",
                            new AttributeValue { N = tuple.MinimumCuteness.ToString(CultureInfo.InvariantCulture) }
                        }
                    },
                    KeyConditionExpression = "#Id = :p1",
                    KeyConditions = null,
                    ConditionalOperator = null,
                    AttributesToGet = null,
                    ReturnConsumedCapacity = null,
                    FilterExpression = "#Cuteness > :p2",
                    Limit = 0,
                    ConsistentRead = false,
                    Select = null,
                    ProjectionExpression = null,
                    IndexName = null,
                    QueryFilter = null,
                    ExclusiveStartKey = new Dictionary<string, AttributeValue>(),
                    IsLimitSet = false,
                    ScanIndexForward = false
                });
        });
    }
}

[DynamoDBMarshaller(typeof(Cat), PropertyName = "QueryWithCuteness",
    ArgumentType = typeof((Guid Id, double MinimumCuteness)))]
[DynamoDBMarshaller(typeof(Cat), PropertyName = "GetByCompositeKey", ArgumentType = typeof((Guid Id, Guid HomeId)))]
[DynamoDBMarshaller(typeof(Cat), PropertyName = "GetById", ArgumentType = typeof(Guid))]
[DynamoDBMarshaller(typeof(Cat), PropertyName = "GetByInvalidPartition", ArgumentType = typeof(string))]
[DynamoDBMarshaller(typeof(Cat), PropertyName = "GetByCompositeInvalidPartition",
    ArgumentType = typeof((string Id, Guid HomeId)))]
[DynamoDBMarshaller(typeof(Cat), PropertyName = "GetByCompositeInvalidRange",
    ArgumentType = typeof((Guid Id, string HomeId)))]
[DynamoDBMarshaller(typeof(Cat), PropertyName = "GetByCompositeInvalidPartitionAndRange",
    ArgumentType = typeof((double Id, string HomeId)))]
public readonly partial record struct Cat(
    [property: DynamoDBHashKey] Guid Id,
    [property: DynamoDBRangeKey] Guid HomeId,
    string Name,
    double Cuteness)
{
    public static readonly Fixture Fixture = new();
}