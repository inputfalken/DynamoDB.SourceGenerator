using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using FluentAssertions;

namespace Dynatello.Tests;

[DynamoDBMarshaller(typeof(User))]
[DynamoDBMarshaller(typeof(User), PropertyName = "UpdateEmail", ArgumentType = typeof(UpdateUserEmail))]
public partial class ToUpdateItemRequestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ArgumentTypeProvided_WithConditionExpression_ShouldIncludeUpdateAndConditionExpressionFields()
    {
        var updateFirst = UpdateEmail
            .OnTable("TABLE")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.UserEmail}, {x.Metadata.ModifiedAt} = {y.TimeStamp}")
            .WithConditionExpression((x, y) => $"{x.Id} = {y.UserId} AND {x.Email} <> {y.UserEmail}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.UserId, y.UserEmail));

        var conditionFirst = UpdateEmail
            .OnTable("TABLE")
            .WithConditionExpression((x, y) => $"{x.Id} = {y.UserId} AND {x.Email} <> {y.UserEmail}")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.UserEmail}, {x.Metadata.ModifiedAt} = {y.TimeStamp}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.UserId, y.UserEmail));

        _fixture
            .CreateMany<UpdateUserEmail>()
            .Should()
            .AllSatisfy(x => Expected(x, updateFirst));
        _fixture
            .CreateMany<UpdateUserEmail>()
            .Should()
            .AllSatisfy(x => Expected(x, conditionFirst));

        static void Expected(UpdateUserEmail updateUserEmail, UpdateRequestBuilder<UpdateUserEmail> builder) =>
            builder.Build(updateUserEmail)
                .Should()
                .BeEquivalentTo(new UpdateItemRequest
                {
                    ConditionExpression = "#Id = :p3 AND #Email <> :p1",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Email", nameof(User.Email) }, { "#Id", nameof(User.Id) },
                        { "#Metadata.#ModifiedAt", nameof(User.Metadata.ModifiedAt) }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":p1", new AttributeValue { S = updateUserEmail.UserEmail } },
                        { ":p2", new AttributeValue { S = updateUserEmail.TimeStamp.ToString("O") } },
                        { ":p3", new AttributeValue { S = updateUserEmail.UserId } }
                    },
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { nameof(User.Id), new AttributeValue { S = updateUserEmail.UserId } },
                        { nameof(User.Email), new AttributeValue { S = updateUserEmail.UserEmail } }
                    },
                    ReturnValues = null,
                    TableName = "TABLE",
                    UpdateExpression = "SET #Email = :p1, #Metadata.#ModifiedAt = :p2",
                    ReturnConsumedCapacity = null,
                    ConditionalOperator = null,
                    Expected = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValuesOnConditionCheckFailure = null,
                    AttributeUpdates = null
                });
    }

    [Fact]
    public void ArgumentTypeProvided_WithoutConditionExpression_ShouldOnlyIncludeUpdateExpressionFields()
    {
        var builder = UpdateEmail
            .OnTable("TABLE")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.UserEmail}, {x.Metadata.ModifiedAt} = {y.TimeStamp}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.UserId, y.UserEmail));

        _fixture
            .CreateMany<UpdateUserEmail>()
            .Should()
            .AllSatisfy(updateUserEmail => builder
                .Build(updateUserEmail)
                .Should()
                .BeEquivalentTo(new UpdateItemRequest
                {
                    ConditionExpression = null,
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Email", nameof(User.Email) },
                        { "#Metadata.#ModifiedAt", nameof(User.Metadata.ModifiedAt) }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {
                            ":p1", new AttributeValue
                            {
                                S = updateUserEmail.UserEmail
                            }
                        },
                        {
                            ":p2", new AttributeValue
                            {
                                S = updateUserEmail.TimeStamp.ToString("O")
                            }
                        }
                    },
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { nameof(User.Id), new AttributeValue { S = updateUserEmail.UserId } },
                        { nameof(User.Email), new AttributeValue { S = updateUserEmail.UserEmail } }
                    },
                    ReturnValues = null,
                    TableName = "TABLE",
                    UpdateExpression = "SET #Email = :p1, #Metadata.#ModifiedAt = :p2",
                    ReturnConsumedCapacity = null,
                    ConditionalOperator = null,
                    Expected = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValuesOnConditionCheckFailure = null,
                    AttributeUpdates = null
                }));
    }

    [Fact]
    public void NoArgumentTypeProvided_WithoutConditionExpression_ShouldOnlyIncludeUpdateExpressionFields()
    {
        var builder = UserMarshaller
            .OnTable("TABLE")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Id, y.Email));

        _fixture.CreateMany<User>()
            .Should()
            .AllSatisfy(user =>
            {
                builder
                    .Build(user)
                    .Should()
                    .BeEquivalentTo(new UpdateItemRequest
                    {
                        ConditionExpression = null,
                        ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            { "#Email", nameof(User.Email) },
                            { "#Firstname", nameof(User.Firstname) }
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            {
                                ":p1", new AttributeValue
                                {
                                    S = user.Email
                                }
                            },
                            {
                                ":p2", new AttributeValue
                                {
                                    S = user.Firstname
                                }
                            }
                        },
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { nameof(User.Id), new AttributeValue { S = user.Id } },
                            { nameof(User.Email), new AttributeValue { S = user.Email } }
                        },
                        ReturnValues = null,
                        TableName = "TABLE",
                        UpdateExpression = "SET #Email = :p1, #Firstname = :p2",
                        ReturnConsumedCapacity = null,
                        ConditionalOperator = null,
                        Expected = null,
                        ReturnItemCollectionMetrics = null,
                        ReturnValuesOnConditionCheckFailure = null,
                        AttributeUpdates = null
                    });
            });
    }

    [Fact]
    public void NoArgumentTypeProvided_WithConditionExpression_ShouldIncludeUpdateAndConditionExpressionFields()
    {
        var user = _fixture.Create<User>();
        var updateItemRequest = UserMarshaller
            .OnTable("TABLE")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}")
            .WithConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Id, y.Lastname))
            .Build(user);

        updateItemRequest.ConditionExpression.Should().Be("#Id = :p3");
        updateItemRequest.ExpressionAttributeNames.Should().HaveCount(3);
        updateItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(user.Email));
        updateItemRequest.ExpressionAttributeNames["#Firstname"].Should().Be(nameof(user.Firstname));
        updateItemRequest.ExpressionAttributeNames["#Id"].Should().Be(nameof(user.Id));
        updateItemRequest.ExpressionAttributeValues.Should().HaveCount(3);
        updateItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(user.Email);
        updateItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(user.Firstname);
        updateItemRequest.ExpressionAttributeValues[":p3"].S.Should().Be(user.Id);
        updateItemRequest.Key[nameof(user.Id)].S.Should().Be(user.Id);
        updateItemRequest.Key[nameof(user.Email)].S.Should().Be(user.Lastname);
        updateItemRequest.ReturnValues.Should().Be(null);
        updateItemRequest.TableName.Should().Be("TABLE");
        updateItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Firstname = :p2");
    }
}