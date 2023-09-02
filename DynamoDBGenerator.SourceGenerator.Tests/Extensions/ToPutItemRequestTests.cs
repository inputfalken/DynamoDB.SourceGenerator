using Amazon.DynamoDBv2;
using AutoFixture;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator.SourceGenerator.Tests.Extensions;

[DynamoDBMarshaller(typeof(User))]
public partial class ToPutItemRequestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Without_ConditionExpression_ShouldNotIncludeExpressionFields()
    {
        var user = _fixture.Create<User>();
        var putItemRequest = UserMarshaller.ToPutItemRequest(user, ReturnValue.NONE, "TABLE");

        putItemRequest.ConditionExpression.Should().BeNullOrWhiteSpace();
        putItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        putItemRequest.TableName.Should().Be("TABLE");

        putItemRequest.ExpressionAttributeNames.Should().BeNullOrEmpty();
        putItemRequest.ExpressionAttributeValues.Should().BeNullOrEmpty();

        putItemRequest.Item.Should().HaveCount(3);
        putItemRequest.Item[nameof(user.Email)].S.Should().Be(user.Email);
        putItemRequest.Item[nameof(user.Firstname)].S.Should().Be(user.Firstname);
        putItemRequest.Item[nameof(user.Lastname)].S.Should().Be(user.Lastname);
    }

    [Fact]
    public void With_ConditionExpression_ShouldIncludeExpressionFields()
    {
        var user = _fixture.Create<User>();
        var putItemRequest = UserMarshaller.ToPutItemRequest(user, (x, y) => $"{x.Email} <> {y.Email} AND {x.Firstname} = {y.Firstname}", ReturnValue.NONE, "TABLE");

        putItemRequest.ConditionExpression.Should().Be("#Email <> :p1 AND #Firstname = :p2");
        putItemRequest.ReturnValues.Should().Be(ReturnValue.NONE);
        putItemRequest.TableName.Should().Be("TABLE");

        putItemRequest.ExpressionAttributeNames.Should().HaveCount(2);
        putItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(user.Email));
        putItemRequest.ExpressionAttributeNames["#Firstname"].Should().Be(nameof(user.Firstname));

        putItemRequest.ExpressionAttributeValues.Should().HaveCount(2);
        putItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(user.Email);
        putItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(user.Firstname);

        putItemRequest.Item.Should().HaveCount(3);
        putItemRequest.Item[nameof(user.Email)].S.Should().Be(user.Email);
        putItemRequest.Item[nameof(user.Firstname)].S.Should().Be(user.Firstname);
        putItemRequest.Item[nameof(user.Lastname)].S.Should().Be(user.Lastname);
    }

}

public class User
{
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
}