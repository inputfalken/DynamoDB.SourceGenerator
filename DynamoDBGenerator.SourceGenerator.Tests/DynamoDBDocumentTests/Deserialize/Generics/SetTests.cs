using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(typeof(StringSetClass))]
[DynamoDBMarshaller(typeof(Int32SetClass))]
public partial class SetTests
{

    [Fact]
    public void Deserialize_EmptyIntSet_IsIncluded()
    {
        Int32SetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int32SetClass.IntSet), new AttributeValue()}})
            .Should()
            .BeOfType<Int32SetClass>()
            .Which
            .IntSet
            .Should()
            .BeEmpty();
    }
    
    [Fact]
    public void Deserialize_ExplicitlyEmptyIntSet_IsIncluded()
    {
        Int32SetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int32SetClass.IntSet), new AttributeValue {NS = new List<string>()}}})
            .Should()
            .BeOfType<Int32SetClass>()
            .Which
            .IntSet
            .Should()
            .BeEmpty();
    }
    
    [Fact]
    public void Serialize_EmptyStringSet_IsIncluded()
    {
        StringSetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringSetClass.StringSet), new AttributeValue()}})
            .Should()
            .BeOfType<StringSetClass>()
            .Which
            .StringSet
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Deserialize_ExplicitlyEmptyStringSet_IsIncluded()
    {
        StringSetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringSetClass.StringSet), new AttributeValue {SS = new List<string>()}}})
            .Should()
            .BeOfType<StringSetClass>()
            .Which
            .StringSet
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Deserialize_IntSetWithValues_IsIncluded()
    {
        Int32SetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int32SetClass.IntSet), new AttributeValue {NS = new List<string> {"1", "2", "3"}}}})
            .Should()
            .BeOfType<Int32SetClass>()
            .Which
            .IntSet
            .Should().SatisfyRespectively(x => x.Should().Be(1), x => x.Should().Be(2), x => x.Should().Be(3));
    }

    [Fact]
    public void Deserialize_NullIntSet_IsSkipped()
    {
        Int32SetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>())
            .Should()
            .BeOfType<Int32SetClass>()
            .Which
            .IntSet
            .Should()
            .BeNull();
    }

    [Fact]
    public void Deserialize_NullStringSet_IsSkipped()
    {
        StringSetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>())
            .Should()
            .BeOfType<StringSetClass>()
            .Which
            .StringSet
            .Should()
            .BeNull();
    }

    [Fact]
    public void Deserialize_StringSetWithValues_IsIncluded()
    {

        StringSetClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(StringSetClass.StringSet), new AttributeValue {SS = new List<string> {"1", "2", "3"}}}})
            .Should()
            .BeOfType<StringSetClass>()
            .Which
            .StringSet
            .Should().SatisfyRespectively(x => x.Should().Be("1"), x => x.Should().Be("2"), x => x.Should().Be("3"));

    }
}

// Could potentially add tests for int64 etc...
public class Int32SetClass
{
    [DynamoDBProperty]
    public HashSet<int>? IntSet { get; set; }
}

public class StringSetClass
{
    [DynamoDBProperty]
    public HashSet<string>? StringSet { get; set; }
}