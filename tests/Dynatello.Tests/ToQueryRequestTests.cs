using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using AutoFixture.Dsl;
using DynamoDBGenerator.Attributes;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using FluentAssertions;

namespace Dynatello.Tests;

public class ToQueryRequestTests
{

    static ToQueryRequestTests()
    {
        var withKeyConditionExpression = Cat.QueryWithCuteness
            .OnTable("TABLE")
            .WithKeyConditionExpression((x, y) => $"{x.Id} = {y.Id}");
        QueryCatWithId = withKeyConditionExpression.ToQueryRequestBuilder();
        
        QueryCatWithIdAndMinimumCuteness = withKeyConditionExpression
            .WithFilterExpression((x, y) => $"{x.Cuteness} > {y.MinimumCuteness}")
            .ToQueryRequestBuilder();
    }

    private static readonly QueryRequestBuilder<(Guid Id, double MinimumCuteness)> QueryCatWithIdAndMinimumCuteness;
    private static readonly QueryRequestBuilder<(Guid Id, double MinimumCuteness)> QueryCatWithId;

    [Fact]
    public void Build_Request()
    {
        Cat.Fixture.CreateMany<(Guid Id, double MinimumCuteness)>(10).Should().AllSatisfy(tuple =>
        {
            var request = QueryCatWithId.Build(tuple);

            request.TableName.Should().Be("TABLE");
            request.ExpressionAttributeNames.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "#Id", nameof(Cat.Id) }
            });

            request.ExpressionAttributeValues.Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
            {
                { ":p1", new AttributeValue { S = tuple.Id.ToString() } },
            });

            request.KeyConditionExpression.Should().Be("#Id = :p1");
        });
    }

    [Fact]
    public void Build_Request_FilterExpression()
    {
        Cat.Fixture.CreateMany<(Guid Id, double MinimumCuteness)>(10).Should().AllSatisfy(tuple =>
        {
            var request = QueryCatWithIdAndMinimumCuteness.Build(tuple);

            request.TableName.Should().Be("TABLE");
            request.ExpressionAttributeNames.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "#Id", nameof(Cat.Id) },
                { "#Cuteness", nameof(Cat.Cuteness) }
            });

            request.ExpressionAttributeValues.Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
            {
                { ":p1", new AttributeValue { S = tuple.Id.ToString() } },
                { ":p2", new AttributeValue { N = tuple.MinimumCuteness.ToString(CultureInfo.InvariantCulture) } }
            });

            request.KeyConditionExpression.Should().Be("#Id = :p1");
            request.FilterExpression.Should().Be("#Cuteness > :p2");
        });
    }
}

[DynamoDBMarshaller(typeof(Cat), PropertyName = "QueryWithCuteness",
    ArgumentType = typeof((Guid Id, double MinimumCuteness)))]
public readonly partial record struct Cat(
    [property: DynamoDBHashKey] Guid Id,
    [property: DynamoDBRangeKey] Guid HomeId,
    string Name,
    double Cuteness)
{
    public static readonly Fixture Fixture = new Fixture();

}