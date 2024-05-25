using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(EntityType = typeof(StringedCollectionClass))]
public partial class StringCollectionTests
{

    [Fact]
    public void Deserialize_ICollection_ShouldBeListWithCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.CollectionInterface), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .CollectionInterface
            .Should()
            .BeOfType<List<string>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }

    [Fact]
    public void Deserialize_IReadOnlyCollection_ShouldBeArrayWithCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.ReadOnlyCollectionInterface), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .ReadOnlyCollectionInterface
            .Should()
            .BeOfType<string[]>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }

    [Fact]
    public void Deserialize_IList_ShouldBeListWithCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.ListInterface), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .ListInterface
            .Should()
            .BeOfType<List<string>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }

    [Fact]
    public void Deserialize_IReadOnlyList_ShouldBeOfArrayWithCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.ReadOnlyListInterface), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .ReadOnlyListInterface
            .Should()
            .BeOfType<string[]>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }

    [Fact]
    public void Deserialize_List_ShouldBeOfListWithCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.List), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .List
            .Should()
            .BeOfType<List<string>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }

    [Fact]
    public void Deserialize_Array_ShouldBeOfArrayWithCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.Array), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .Array
            .Should()
            .BeOfType<string[]>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }

    [Fact]
    public void Deserialize_IEnumerable_ShouldContainCorrectElements()
    {
        StringedCollectionClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringedCollectionClass.EnumerableInterface), new AttributeValue {L = new List<AttributeValue> {new() {S = "ABC"}, new() {S = "Foo"}}}}})
            .EnumerableInterface
            .Should()
            .SatisfyRespectively(x => x.Should().Be("ABC"), x => x.Should().Be("Foo"));
    }
}

public class StringedCollectionClass
{
    public IEnumerable<string>? EnumerableInterface { get; set; }
    public List<string>? List { get; set; }
    public string[]? Array { get; set; }
    public IList<string>? ListInterface { get; set; }
    public IReadOnlyList<string>? ReadOnlyListInterface { get; set; }
    public ICollection<string>? CollectionInterface { get; set; }
    public IReadOnlyCollection<string>? ReadOnlyCollectionInterface { get; set; }
}
