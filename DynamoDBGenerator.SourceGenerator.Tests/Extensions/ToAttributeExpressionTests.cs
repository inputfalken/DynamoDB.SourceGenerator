using AutoFixture;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator.SourceGenerator.Tests.Extensions;

[DynamoDBMarshaller(typeof(OrderAttributeExpressionTests))]
public partial class ToAttributeExpressionTests
{
    private readonly Fixture _fixture = new();

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
    public string Id { get; set; }

    public decimal TotalPrice { get; set; }
    public string ClientId { get; set; }

}