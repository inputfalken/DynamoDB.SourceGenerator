using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using FluentAssertions;

namespace Dynatello.Tests;

[DynamoDBMarshaller(typeof(User))]
public partial class ToPutItemRequestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Without_ConditionExpression_ShouldNotIncludeExpressionFields()
    {
        var builder = UserMarshaller
            .OnTable("TABLE")
            .ToPutRequestBuilder();
        _fixture.CreateMany<User>().Should().AllSatisfy(user =>
        {
            builder.Build(user)
                .Should()
                .BeEquivalentTo(new PutItemRequest
                {
                    ConditionExpression = null,
                    ExpressionAttributeNames = null,
                    ExpressionAttributeValues = null,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { nameof(user.Email), new AttributeValue { S = user.Email } },
                        { nameof(user.Firstname), new AttributeValue { S = user.Firstname } },
                        { nameof(user.Lastname), new AttributeValue { S = user.Lastname } },
                        { nameof(user.Id), new AttributeValue { S = user.Id } },
                        {
                            nameof(user.Metadata), new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {
                                        nameof(user.Metadata.ModifiedAt),
                                        new AttributeValue { S = user.Metadata.ModifiedAt.ToString("O") }
                                    }
                                }
                            }
                        },
                    },
                    ReturnValues = null,
                    TableName = "TABLE",
                    Expected = null,
                    ReturnConsumedCapacity = null,
                    ConditionalOperator = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValuesOnConditionCheckFailure = null
                });
        });
    }

    [Fact]
    public void With_ConditionExpression_ShouldIncludeExpressionFields()
    {
        var builder = UserMarshaller
            .OnTable("TABLE")
            .WithConditionExpression((x, y) => $"{x.Email} <> {y.Email} AND {x.Firstname} = {y.Firstname}")
            .ToPutRequestBuilder();
        
        _fixture.CreateMany<User>().Should().AllSatisfy(user =>
        {
            builder.Build(user)
                .Should()
                .BeEquivalentTo(new PutItemRequest
                {
                    ConditionExpression = "#Email <> :p1 AND #Firstname = :p2",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Email", nameof(user.Email) },
                        { "#Firstname", nameof(user.Firstname) }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":p1", new AttributeValue { S = user.Email } },
                        { ":p2", new AttributeValue { S = user.Firstname } }
                    },
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { nameof(user.Email), new AttributeValue { S = user.Email } },
                        { nameof(user.Firstname), new AttributeValue { S = user.Firstname } },
                        { nameof(user.Lastname), new AttributeValue { S = user.Lastname } },
                        { nameof(user.Id), new AttributeValue { S = user.Id } },
                        {
                            nameof(user.Metadata), new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {
                                        nameof(user.Metadata.ModifiedAt),
                                        new AttributeValue { S = user.Metadata.ModifiedAt.ToString("O") }
                                    }
                                }
                            }
                        },
                    },
                    ReturnValues = null,
                    TableName = "TABLE",
                    Expected = null,
                    ReturnConsumedCapacity = null,
                    ConditionalOperator = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValuesOnConditionCheckFailure = null
                });
        });
    }
}

public class User
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;

    [DynamoDBRangeKey]
    public string Email { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public Meta Metadata { get; set; } = null!;

    public class Meta
    {
        public DateTimeOffset ModifiedAt { get; set; }
    }
}

public class UpdateUserEmail
{
    public string UserId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public DateTimeOffset TimeStamp { get; set; }
}