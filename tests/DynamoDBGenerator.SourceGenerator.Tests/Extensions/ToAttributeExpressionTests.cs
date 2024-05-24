using AutoFixture;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator.SourceGenerator.Tests.Extensions;

[DynamoDBMarshaller(EntityType = typeof(OrderAttributeExpressionTests))]
public partial class ToAttributeExpressionTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ToAttributeExpression_Single_ObjectAssignmentExpression()
    {
        var order = _fixture.Create<OrderAttributeExpressionTests>();
        var result = OrderAttributeExpressionTestsMarshaller
            .ToAttributeExpression(order, (x, y) => $"SET {x.UserEntity} = {y.UserEntity}");

        result.Values
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(":p1");
                x.Value.M.Should().SatisfyRespectively(
                    y =>
                    {
                        y.Key.Should().Be(nameof(OrderAttributeExpressionTests.UserEntity.Id));
                        y.Value.S.Should().Be(order.UserEntity.Id);
                    }, y =>
                    {

                        y.Key.Should().Be(nameof(OrderAttributeExpressionTests.UserEntity.DisplayName));
                        y.Value.S.Should().Be(order.UserEntity.DisplayName);
                    });
            });

        result.Names.Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be("#User");
            x.Value.Should().Be("User");
        });
        result.Expressions.Should().SatisfyRespectively(x => x.Should().Be("SET #User = :p1"));
    }

    [Fact]
    public void ToAttributeExpression_Double_ObjectAssignmentExpression()
    {
        var order = _fixture.Create<OrderAttributeExpressionTests>();
        var result = OrderAttributeExpressionTestsMarshaller
            .ToAttributeExpression(order, (x, y) => $"SET {x.UserEntity} = {y.UserEntity}", (x, y) => $"{x.Id} = {y.Id}");

        // Order of Values and Names do not match the access pattern in the expression.
        // Their order is based on the the order of the yield returns of AccessedValues and AccessedNames.
        result.Values
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(":p2");
                    x.Value.S.Should().Be(order.Id);
                },
                x =>
                {
                    x.Key.Should().Be(":p1");
                    x.Value.M.Should().SatisfyRespectively(
                        y =>
                        {
                            y.Key.Should().Be(nameof(OrderAttributeExpressionTests.UserEntity.Id));
                            y.Value.S.Should().Be(order.UserEntity.Id);
                        }, y =>
                        {

                            y.Key.Should().Be(nameof(OrderAttributeExpressionTests.UserEntity.DisplayName));
                            y.Value.S.Should().Be(order.UserEntity.DisplayName);
                        });
                }
            );

        result.Names
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be("#Id");
                    x.Value.Should().Be("Id");
                },
                x =>
                {
                    x.Key.Should().Be("#User");
                    x.Value.Should().Be("User");
                }
            );
        result.Expressions.Should().SatisfyRespectively(x => x.Should().Be("SET #User = :p1"), x => x.Should().Be("#Id = :p2"));
    }

    [Fact]
    public void ToAttributeExpression_Single_Expression()
    {
        var order = _fixture.Create<OrderAttributeExpressionTests>();
        var result = OrderAttributeExpressionTestsMarshaller
            .ToAttributeExpression(order, (x, y) => $"{x.Id} <> {y.Id}");

        result.Expressions
            .Should()
            .SatisfyRespectively(x => x.Should().Be("#Id <> :p1"));
        result.Names
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be("#Id");
                x.Value.Should().Be("Id");
            });
        result.Values
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(":p1");
                x.Value.S.Should().Be(order.Id);
            });
    }

    [Fact]
    public void ToAttributeExpression_Double_Expressions()
    {
        var order = _fixture.Create<OrderAttributeExpressionTests>();
        var result = OrderAttributeExpressionTestsMarshaller
            .ToAttributeExpression(
                order,
                (x, y) => $"{x.Id} <> {y.Id}",
                (x, y) => $"SET {x.Id} = {y.Id}"
            );

        result.Expressions
            .Should()
            .SatisfyRespectively(
                x => x.Should().Be("#Id <> :p1"),
                x => x.Should().Be("SET #Id = :p1")
            );
        result.Names
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be("#Id");
                x.Value.Should().Be("Id");
            });

        result.Values
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(":p1");
                x.Value.S.Should().Be(order.Id);
            });
    }
}

public class OrderAttributeExpressionTests
{
    public string Id { get; set; } = null!;

    public decimal TotalPrice { get; set; }
    public string ClientId { get; set; } = null!;

    [DynamoDBProperty("User")]
    public User UserEntity { get; set; } = null!;

    public class User
    {
        public string Id { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
    }

}
