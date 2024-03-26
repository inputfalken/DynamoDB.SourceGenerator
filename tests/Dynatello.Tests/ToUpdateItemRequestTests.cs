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
                    AttributeUpdates = null,
                    ConditionalOperator = null,
                    ConditionExpression = "#Id = :p3 AND #Email <> :p1",
                    Expected = null,
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
                    ReturnConsumedCapacity = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValues = null,
                    ReturnValuesOnConditionCheckFailure = null,
                    TableName = "TABLE",
                    UpdateExpression = "SET #Email = :p1, #Metadata.#ModifiedAt = :p2"
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
                    AttributeUpdates = null,
                    ConditionalOperator = null,
                    ConditionExpression = null,
                    Expected = null,
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Email", nameof(User.Email) },
                        { "#Metadata.#ModifiedAt", nameof(User.Metadata.ModifiedAt) }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":p1", new AttributeValue { S = updateUserEmail.UserEmail } },
                        { ":p2", new AttributeValue { S = updateUserEmail.TimeStamp.ToString("O") } }
                    },
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { nameof(User.Id), new AttributeValue { S = updateUserEmail.UserId } },
                        { nameof(User.Email), new AttributeValue { S = updateUserEmail.UserEmail } }
                    },
                    ReturnConsumedCapacity = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValues = null,
                    ReturnValuesOnConditionCheckFailure = null,
                    TableName = "TABLE",
                    UpdateExpression = "SET #Email = :p1, #Metadata.#ModifiedAt = :p2"
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
            .AllSatisfy(x => builder
                .Build(x)
                .Should()
                .BeEquivalentTo(new UpdateItemRequest
                {
                    AttributeUpdates = null,
                    ConditionalOperator = null,
                    ConditionExpression = null,
                    Expected = null,
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#Email", nameof(User.Email) },
                        { "#Firstname", nameof(User.Firstname) }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":p1", new AttributeValue { S = x.Email } },
                        { ":p2", new AttributeValue { S = x.Firstname } }
                    },
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { nameof(User.Id), new AttributeValue { S = x.Id } },
                        { nameof(User.Email), new AttributeValue { S = x.Email } }
                    },
                    ReturnConsumedCapacity = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValues = null,
                    ReturnValuesOnConditionCheckFailure = null,
                    TableName = "TABLE",
                    UpdateExpression = "SET #Email = :p1, #Firstname = :p2"
                }));
    }

    [Fact]
    public void NoArgumentTypeProvided_WithConditionExpression_ShouldIncludeUpdateAndConditionExpressionFields()
    {
        var updateFirst = UserMarshaller
            .OnTable("TABLE")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}")
            .WithConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Id, y.Email));

        var conditionFirst = UserMarshaller
            .OnTable("TABLE")
            .WithUpdateExpression((x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}")
            .WithConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Id, y.Email));

        _fixture.CreateMany<User>().Should().AllSatisfy(x => Expected(x, conditionFirst));
        _fixture.CreateMany<User>().Should().AllSatisfy(x => Expected(x, updateFirst));

        static void Expected(User x, UpdateRequestBuilder<User> builder) =>
            builder
                .Build(x)
                .Should()
                .BeEquivalentTo(new UpdateItemRequest
                {
                    AttributeUpdates = null,
                    ConditionalOperator = null,
                    ConditionExpression = "#Id = :p3",
                    Expected = null,
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        { "#Email", "Email" },
                        { "#Firstname", "Firstname" },
                        { "#Id", "Id" }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        { ":p1", new AttributeValue { S = x.Email } },
                        { ":p2", new AttributeValue { S = x.Firstname } }, { ":p3", new AttributeValue { S = x.Id } }
                    },
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "Id", new AttributeValue { S = x.Id } },
                        { "Email", new AttributeValue { S = x.Email } }
                    },
                    ReturnConsumedCapacity = null,
                    ReturnItemCollectionMetrics = null,
                    ReturnValues = null,
                    ReturnValuesOnConditionCheckFailure = null,
                    TableName = "TABLE",
                    UpdateExpression = "SET #Email = :p1, #Firstname = :p2"
                });
    }
}