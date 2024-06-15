using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(EntityType = typeof(Entity), AccessName = "DifferentArgument", ArgumentType = typeof(Argument))]
[DynamoDBMarshaller(EntityType = typeof(Entity), AccessName = "ImplicitArgument")]
public partial class MapArgumentTests
{
    private static readonly Fixture Fixture = new Fixture();

    [Fact]
    public void UnmarshallArgument_DifferentArgument_ShouldUnmarshall()
    {
        var argument = Fixture.Create<Argument>();
        DifferentArgument
          .UnmarshallArgument(new Dictionary<string, AttributeValue>(){
              {nameof(Argument.Id), new AttributeValue(){S = argument.Id}},
              {nameof(Argument.Age), new AttributeValue(){N = argument.Age.ToString()}}
              })
          .Should()
          .BeEquivalentTo(argument);

    }

    [Fact]
    public void UnmarshallArgument_ImplicitArgument_ShouldUnmarshall()
    {
        var argument = Fixture.Create<Argument>();
        ImplicitArgument
          .UnmarshallArgument(new Dictionary<string, AttributeValue>(){
              {nameof(Entity.Id), new AttributeValue(){S = argument.Id}},
              {nameof(Entity.Age), new AttributeValue(){N = argument.Age.ToString()}},
              {nameof(Entity.CreatedAt), new AttributeValue(){N = argument.Age.ToString()}}
              })
          .Should()
          .BeEquivalentTo(argument);
    }

    public record Argument(string Id, int Age);
    public record Entity(string Id, int Age, DateTime CreatedAt);
}

