using Amazon.DynamoDBv2.DataModel;
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
        var user = _fixture.Create<User>();
        var putItemRequest = UserMarshaller.OnTable("TABLE")
            .ToPutRequestBuilder()
            .Build(user);

        putItemRequest.ConditionExpression.Should().BeNullOrWhiteSpace();
        putItemRequest.ExpressionAttributeNames.Should().BeNullOrEmpty();
        putItemRequest.ExpressionAttributeValues.Should().BeNullOrEmpty();
        putItemRequest.Item.Should().HaveCount(5);
        putItemRequest.Item[nameof(user.Email)].S.Should().Be(user.Email);
        putItemRequest.Item[nameof(user.Firstname)].S.Should().Be(user.Firstname);
        putItemRequest.Item[nameof(user.Lastname)].S.Should().Be(user.Lastname);
        putItemRequest.Item[nameof(user.Id)].S.Should().Be(user.Id);
        putItemRequest.Item[nameof(user.Metadata)].M.Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(user.Metadata.ModifiedAt));
            x.Value.S.Should().Be(user.Metadata.ModifiedAt.ToString("O"));
        });
        putItemRequest.ReturnValues.Should().Be(null);
        putItemRequest.TableName.Should().Be("TABLE");
    }

    [Fact]
    public void With_ConditionExpression_ShouldIncludeExpressionFields()
    {
        var user = _fixture.Create<User>();
        var putItemRequest = UserMarshaller
            .OnTable("TABLE")
            .WithConditionExpression((x, y) => $"{x.Email} <> {y.Email} AND {x.Firstname} = {y.Firstname}")
            .ToPutRequestBuilder()
            .Build(user);

        putItemRequest.ConditionExpression.Should().Be("#Email <> :p1 AND #Firstname = :p2");
        putItemRequest.ExpressionAttributeNames.Should().HaveCount(2);
        putItemRequest.ExpressionAttributeNames["#Email"].Should().Be(nameof(user.Email));
        putItemRequest.ExpressionAttributeNames["#Firstname"].Should().Be(nameof(user.Firstname));
        putItemRequest.ExpressionAttributeValues.Should().HaveCount(2);
        putItemRequest.ExpressionAttributeValues[":p1"].S.Should().Be(user.Email);
        putItemRequest.ExpressionAttributeValues[":p2"].S.Should().Be(user.Firstname);
        putItemRequest.Item.Should().HaveCount(5);
        putItemRequest.Item[nameof(user.Email)].S.Should().Be(user.Email);
        putItemRequest.Item[nameof(user.Firstname)].S.Should().Be(user.Firstname);
        putItemRequest.Item[nameof(user.Lastname)].S.Should().Be(user.Lastname);
        putItemRequest.Item[nameof(user.Id)].S.Should().Be(user.Id);
        putItemRequest.Item[nameof(user.Metadata)].M.Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(user.Metadata.ModifiedAt));
            x.Value.S.Should().Be(user.Metadata.ModifiedAt.ToString("O"));
        });
        putItemRequest.ReturnValues.Should().Be(null);
        putItemRequest.TableName.Should().Be("TABLE");
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