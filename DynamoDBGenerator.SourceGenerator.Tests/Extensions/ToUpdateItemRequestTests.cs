using Amazon.DynamoDBv2;
using AutoFixture;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator.SourceGenerator.Tests.Extensions;

[DynamoDBMarshaller(typeof(User))]
[DynamoDBMarshaller(typeof(User), PropertyName = "UpdateEmail", ArgumentType = typeof(UpdateUserEmail))]
public partial class ToUpdateItemRequestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ArgumentTypeProvided_WithConditionExpression_ShouldIncludeUpdateAndConditionExpressionFields()
    {
        var updateUserEmail = _fixture.Create<UpdateUserEmail>();
        var updateItemRequest = UpdateEmail.ToUpdateItemRequest(
            updateUserEmail,
            (x, y) => x.Keys(y.UserId, y.UserEmail),
            (x, y) => $"SET {x.Email} = {y.UserEmail}, {x.Metadata.ModifiedAt} = {y.TimeStamp}",
            (x,y) => $"{x.Id} = {y.UserId} AND {x.Email} <> {y.UserEmail}",
            ReturnValue.NONE,
            "TABLE"
        );

        updateItemRequest.ConditionExpression.Should().Be("#Id = :p3 AND #Email <> :p1");
        updateItemRequest.ExpressionAttributeNames.Should().HaveCount(3);
        updateItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(User.Email));
        updateItemRequest.ExpressionAttributeNames["#Id"].Should().Be(nameof(User.Id));
        updateItemRequest.ExpressionAttributeNames["#Metadata.#ModifiedAt"].Should().Be(nameof(User.Metadata.ModifiedAt));
        updateItemRequest.ExpressionAttributeValues.Should().HaveCount(3);
        updateItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(updateUserEmail.UserEmail);
        updateItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(updateUserEmail.TimeStamp.ToString("O"));
        updateItemRequest.ExpressionAttributeValues[":p3"].S.Should().Be(updateUserEmail.UserId);
        updateItemRequest.Key[nameof(User.Email)].S.Should().Be(updateUserEmail.UserEmail);
        updateItemRequest.Key[nameof(User.Id)].S.Should().Be(updateUserEmail.UserId);
        updateItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        updateItemRequest.TableName.Should().Be("TABLE");
        updateItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Metadata.#ModifiedAt = :p2");
    }

    [Fact]
    public void ArgumentTypeProvided_WithoutConditionExpression_ShouldOnlyIncludeUpdateExpressionFields()
    {
        var updateUserEmail = _fixture.Create<UpdateUserEmail>();
        var updateItemRequest = UpdateEmail.ToUpdateItemRequest(
            updateUserEmail,
            (x, y) => x.Keys(y.UserId, y.UserEmail),
            (x, y) => $"SET {x.Email} = {y.UserEmail}, {x.Metadata.ModifiedAt} = {y.TimeStamp}",
            ReturnValue.NONE,
            "TABLE"
        );

        updateItemRequest.ConditionExpression.Should().BeNullOrWhiteSpace();
        updateItemRequest.ExpressionAttributeNames.Should().HaveCount(2);
        updateItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(User.Email));
        updateItemRequest.ExpressionAttributeNames["#Metadata.#ModifiedAt"].Should().Be(nameof(User.Metadata.ModifiedAt));
        updateItemRequest.ExpressionAttributeValues.Should().HaveCount(2);
        updateItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(updateUserEmail.UserEmail);
        updateItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(updateUserEmail.TimeStamp.ToString("O"));
        updateItemRequest.Key[nameof(User.Email)].S.Should().Be(updateUserEmail.UserEmail);
        updateItemRequest.Key[nameof(User.Id)].S.Should().Be(updateUserEmail.UserId);
        updateItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        updateItemRequest.TableName.Should().Be("TABLE");
        updateItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Metadata.#ModifiedAt = :p2");
    }

    [Fact]
    public void NoArgumentTypeProvided_WithoutConditionExpression_ShouldOnlyIncludeUpdateExpressionFields()
    {
        var user = _fixture.Create<User>();
        var updateItemRequest = UserMarshaller
            .ToUpdateItemRequest(
                user,
                (x, y) => x.Keys(y.Id, y.Lastname),
                (x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}",
                ReturnValue.NONE,
                "TABLE"
            );

        updateItemRequest.ConditionExpression.Should().BeNullOrWhiteSpace();
        updateItemRequest.ExpressionAttributeNames.Should().HaveCount(2);
        updateItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(user.Email));
        updateItemRequest.ExpressionAttributeNames["#Firstname"].Should().Be(nameof(user.Firstname));
        updateItemRequest.ExpressionAttributeValues.Should().HaveCount(2);
        updateItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(user.Email);
        updateItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(user.Firstname);
        updateItemRequest.Key[nameof(user.Id)].S.Should().Be(user.Id);
        updateItemRequest.Key[nameof(user.Email)].S.Should().Be(user.Lastname);
        updateItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        updateItemRequest.TableName.Should().Be("TABLE");
        updateItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Firstname = :p2");
    }

    [Fact]
    public void NoArgumentTypeProvided_WithConditionExpression_ShouldIncludeUpdateAndConditionExpressionFields()
    {
        var user = _fixture.Create<User>();
        var updateItemRequest = UserMarshaller
            .ToUpdateItemRequest(
                user,
                (x, y) => x.Keys(y.Id, y.Lastname),
                (x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}",
                (x, y) => $"{x.Id} = {y.Id}",
                ReturnValue.NONE,
                "TABLE"
            );

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
        updateItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        updateItemRequest.TableName.Should().Be("TABLE");
        updateItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Firstname = :p2");
    }
}