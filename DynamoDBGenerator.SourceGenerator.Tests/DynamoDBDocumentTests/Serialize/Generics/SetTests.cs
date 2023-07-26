namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBDocument(typeof(StringSetClass))]
[DynamoDBDocument(typeof(Int32SetClass))]
public partial class SetTests
{

    [Fact]
    public void Serialize_EmptyIntSet_IsIncluded()
    {
        var @class = new Int32SetClass
        {
            IntSet = new HashSet<int>()
        };

        Int32SetClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int32SetClass.IntSet));
    }
    [Fact]
    public void Serialize_EmptyStringSet_IsIncluded()
    {
        var @class = new StringSetClass
        {
            StringSet = new HashSet<string>()
        };

        StringSetClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(StringSetClass.StringSet));
    }

    [Fact]
    public void Serialize_IntSetWithValues_IsIncluded()
    {
        var @class = new Int32SetClass
        {
            IntSet = new HashSet<int>(new[] {1, 2, 3})
        };

        Int32SetClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int32SetClass.IntSet))
            .And
            .AllSatisfy(x => x.Value.NS.Should()
                .SatisfyRespectively(
                    y => y.Should().Be("1"),
                    y => y.Should().Be("2"),
                    y => y.Should().Be("3")
                )
            );
    }

    [Fact]
    public void Serialize_NullIntSet_IsSkipped()
    {
        var @class = new Int32SetClass
        {
            IntSet = null
        };

        Int32SetClassDocument
            .Serialize(@class)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Serialize_NullStringSet_IsSkipped()
    {
        var @class = new StringSetClass
        {
            StringSet = null
        };

        StringSetClassDocument
            .Serialize(@class)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Serialize_StringSetWithValues_IsIncluded()
    {
        var @class = new StringSetClass
        {
            StringSet = new HashSet<string>(new[] {"1", "2", "3"})
        };

        StringSetClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(StringSetClass.StringSet))
            .And
            .AllSatisfy(x => x.Value.SS.Should()
                .SatisfyRespectively(
                    y => y.Should().Be("1"),
                    y => y.Should().Be("2"),
                    y => y.Should().Be("3")
                )
            );
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
