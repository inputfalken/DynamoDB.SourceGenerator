using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Tuples;

[DynamoDBMarshaller(typeof(Animal), ArgumentType = typeof((string Id, Animal.Status Status, DateTime TimeStamp)), PropertyName = "SetStatus")]
[DynamoDBMarshaller(typeof(Animal), PropertyName = "SaveAnimal", ArgumentType = typeof((Animal animal, Animal.Status adopted, Animal.Status pending)))]
public partial class TupleArgumentTests
{

    private static readonly Fixture Fixture;
    static TupleArgumentTests()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        Fixture = fixture;
    }

    private static Dictionary<string, AttributeValue> MapToAttributeValue(Animal animal)
    {
        return new Dictionary<string, AttributeValue>
        {
            {nameof(Animal.Id), new AttributeValue {S = animal.Id}},
            {nameof(Animal.Name), new AttributeValue {S = animal.Name}},
            {nameof(Animal.Siblings), new AttributeValue {L = animal.Siblings.Select(x => new AttributeValue {M = MapToAttributeValue(x)}).ToList()}},
            {
                nameof(Animal.From), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        {nameof(Animal.From.Country), new AttributeValue {S = animal.From.Country}}
                    }
                }
            },
            {nameof(Animal.AdoptionStatus), new AttributeValue {N = ((int)animal.AdoptionStatus).ToString()}},
            {
                nameof(Animal.Metadata), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        {nameof(animal.Metadata.StatusSetAt), new AttributeValue {S = animal.Metadata.StatusSetAt?.ToString("O")}},
                        {nameof(animal.Metadata.CreatedAt), new AttributeValue {S = animal.Metadata.CreatedAt.ToString("O")}}
                    }
                }
            }
        };

    }

    [Fact]
    public void Marshall_SaveAnimal()
    {
        var animalBuilder = Fixture.Build<Animal>();
        var animal = animalBuilder
            .With(x => x.Siblings, new[] {animalBuilder.Create(), animalBuilder.Create(), animalBuilder.Create()}).Create();

        SaveAnimal
            .Marshall(animal)
            .Should()
            .BeEquivalentTo(MapToAttributeValue(animal));
    }


    [Fact]
    public void Marshall_SetStatus()
    {
        var animalBuilder = Fixture.Build<Animal>();
        var animal = animalBuilder
            .With(x => x.Siblings, new[] {animalBuilder.Create(), animalBuilder.Create(), animalBuilder.Create()}).Create();

        SetStatus
            .Marshall(animal)
            .Should()
            .BeEquivalentTo(MapToAttributeValue(animal));
    }


    [Fact]
    public void UnMarshall_SetStatus()
    {
        var animalBuilder = Fixture.Build<Animal>();
        var animal = animalBuilder
            .With(x => x.Siblings, new[] {animalBuilder.Create(), animalBuilder.Create(), animalBuilder.Create()}).Create();

        SetStatus
            .Unmarshall(MapToAttributeValue(animal))
            .Should()
            .BeEquivalentTo(animal);

    }
    [Fact]
    public void UnMarshall_SaveAnimal()
    {
        var animalBuilder = Fixture.Build<Animal>();
        var animal = animalBuilder
            .With(x => x.Siblings, new[] {animalBuilder.Create(), animalBuilder.Create(), animalBuilder.Create()}).Create();

        SaveAnimal
            .Unmarshall(MapToAttributeValue(animal))
            .Should()
            .BeEquivalentTo(animal);

    }

    [Fact]
    public void Interchangeable_SetStatus()
    {
        var animalBuilder = Fixture.Build<Animal>();
        var animal = animalBuilder
            .With(x => x.Siblings, new[] {animalBuilder.Create(), animalBuilder.Create(), animalBuilder.Create()}).Create();

        SetStatus.Unmarshall(SetStatus.Marshall(animal)).Should().BeEquivalentTo(animal);
    }

    [Fact]
    public void Interchangeable_SaveAnimal()
    {
        var animalBuilder = Fixture.Build<Animal>();
        var animal = animalBuilder
            .With(x => x.Siblings, new[] {animalBuilder.Create(), animalBuilder.Create(), animalBuilder.Create()}).Create();

        SaveAnimal.Unmarshall(SaveAnimal.Marshall(animal)).Should().BeEquivalentTo(animal);
    }

    [Fact]
    public void ToAttributeExpression_SetStatus()
    {
        var timeStamp = DateTime.Now;
        var cases = Enum.GetValues<Animal.Status>().Select((status, i) =>
            (AttributeExpressions: SetStatus.ToAttributeExpression(
                    (i.ToString(), status, timeStamp),
                    (x, y) => $"{x.Id} = {y.Id} AND {x.AdoptionStatus} <> {y.Status}",
                    (x, y) => $"SET {x.AdoptionStatus} = {y.Status}, {x.Metadata.StatusSetAt} = {y.TimeStamp}"
                ),
                ExpectedId: i.ToString(),
                Status: status)
        );

        cases.Should().AllSatisfy(o =>
        {
            var (attributeExpression, expectedId, status) = o;

            attributeExpression.Expressions.Should().BeEquivalentTo("#Id = :p1 AND #AdoptionStatus <> :p2", "SET #AdoptionStatus = :p2, #Metadata.#StatusSetAt = :p3");

            attributeExpression.Names.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                {"#Id", "Id"},
                {"#AdoptionStatus", "AdoptionStatus"},
                {"#Metadata.#StatusSetAt", "StatusSetAt"}
            });

            attributeExpression.Values.Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
            {
                {":p1", new AttributeValue {S = expectedId}},
                {":p2", new AttributeValue {N = ((int)status).ToString()}},
                {":p3", new AttributeValue {S = timeStamp.ToString("O")}}
            });
        });

    }
    
    [Fact]
    public void ToAttributeExpression_SaveAnimal()
    {
        var animal = Fixture.Create<Animal>();
        var @case = SaveAnimal.ToAttributeExpression((animal, Animal.Status.Adopted, Animal.Status.Pending), (x, y) => $"{x.Id} = {y.animal.Id} AND ({x.AdoptionStatus} <> {y.adopted} OR {x.AdoptionStatus} <> {y.pending})");

        @case.Expressions.Should().BeEquivalentTo("#Id = :p1 AND (#AdoptionStatus <> :p2 OR #AdoptionStatus <> :p3)");
        @case.Names.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            {"#Id", "Id"},
            {"#AdoptionStatus", "AdoptionStatus"}
        });

        @case.Values.Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
        {
            {":p1", new AttributeValue{S = animal.Id}},
            {":p2", new AttributeValue{N = ((int)Animal.Status.Adopted).ToString()}},
            {":p3", new AttributeValue{N = ((int)Animal.Status.Pending).ToString()}},
        });

    }

    public record Animal(string Id, string Name, IReadOnlyList<Animal> Siblings, Animal.Origin From, Animal.Status AdoptionStatus, Animal.MetaData Metadata)
    {
        public record Origin(string Country);

        public record MetaData(DateTime? StatusSetAt, DateTime CreatedAt);

        public enum Status
        {
            Adopted = 1,
            Pending = 2,
            Denied = 3,
            None = 4
        }
    }
}