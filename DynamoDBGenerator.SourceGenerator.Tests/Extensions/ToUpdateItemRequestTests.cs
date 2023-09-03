using Amazon.DynamoDBv2;
using AutoFixture;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator.SourceGenerator.Tests.Extensions;

[DynamoDBMarshaller(typeof(KeyedUser))]
public partial class ToUpdateItemRequestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Without_ConditionExpression_ShouldNotIncludeExpressionFields()
    {
        var user = _fixture.Create<KeyedUser>();
        var putItemRequest = KeyedUserMarshaller
            .ToUpdateItemRequest(
                user,
                (x, y) => x.PartitionKey(y.Id),
                (x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}",
                ReturnValue.NONE,
                "TABLE"
            );

        putItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Firstname = :p2");
        putItemRequest.ConditionExpression.Should().BeNullOrWhiteSpace();
        putItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        putItemRequest.TableName.Should().Be("TABLE");
        putItemRequest.ExpressionAttributeNames.Should().HaveCount(2);
        putItemRequest.ExpressionAttributeValues.Should().HaveCount(2);
        putItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(user.Email));
        putItemRequest.ExpressionAttributeNames["#Firstname"].Should().Be(nameof(user.Firstname));
        putItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(user.Email);
        putItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(user.Firstname);
    }

    [Fact]
    public void With_ConditionExpression_ShouldIncludeExpressionFields()
    {
        var user = _fixture.Create<KeyedUser>();
        var putItemRequest = KeyedUserMarshaller
            .ToUpdateItemRequest(
                user,
                (x, y) => x.PartitionKey(y.Id),
                (x, y) => $"SET {x.Email} = {y.Email}, {x.Firstname} = {y.Firstname}",
                (x, y) => $"{x.Lastname} <> {y.Lastname}",
                ReturnValue.NONE,
                "TABLE"
            );

        putItemRequest.UpdateExpression.Should().Be("SET #Email = :p1, #Firstname = :p2");
        putItemRequest.ConditionExpression.Should().Be("#Lastname <> :p3");
        putItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        putItemRequest.TableName.Should().Be("TABLE");
        putItemRequest.ExpressionAttributeNames.Should().HaveCount(3);
        putItemRequest.ExpressionAttributeValues.Should().HaveCount(3);
        putItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(user.Email));
        putItemRequest.ExpressionAttributeNames["#Firstname"].Should().Be(nameof(user.Firstname));
        putItemRequest.ExpressionAttributeNames["#Lastname"].Should().Be(nameof(user.Lastname));
        putItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(user.Email);
        putItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(user.Firstname);
        putItemRequest.ExpressionAttributeValues[":p3"].S.Should().Be(user.Lastname);
    }
}

public class KeyedUser
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
}