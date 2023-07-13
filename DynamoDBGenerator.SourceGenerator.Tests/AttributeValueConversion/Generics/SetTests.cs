namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class SetTests
{
    [Fact]
    public void BuildAttributeValues_EmptyStringSet_IsIncluded()
    {
        var @class = new StringSetClass
        {
            StringSet = new HashSet<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(StringSetClass.StringSet));
    }

    [Fact]
    public void BuildAttributeValues_NullStringSet_IsSkipped()
    {
        var @class = new StringSetClass()
        {
            StringSet = null
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_StringSetWithValues_IsIncluded()
    {
        var @class = new StringSetClass
        {
            StringSet = new HashSet<string>(new[] {"1", "2", "3"})
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(StringSetClass.StringSet))
            .And
            .AllSatisfy(x => x.Value.SS
                .Should()
                .SatisfyRespectively(
                    y => y.Should().Be("1"),
                    y => y.Should().Be("2"),
                    y => y.Should().Be("3")
                )
            );
    }

    [Fact]
    public void BuildAttributeValues_EmptyIntSet_IsIncluded()
    {
        var @class = new Int32SetClass()
        {
            IntSet = new HashSet<int>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int32SetClass.IntSet));
    }

    [Fact]
    public void BuildAttributeValues_NullIntSet_IsSkipped()
    {
        var @class = new Int32SetClass()
        {
            IntSet = null
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_IntSetWithValues_IsIncluded()
    {
        var @class = new Int32SetClass()
        {
            IntSet = new HashSet<int>(new[] {1, 2, 3})
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int32SetClass.IntSet))
            .And
            .AllSatisfy(x => x.Value.NS
                .Should()
                .SatisfyRespectively(
                    y => y.Should().Be("1"),
                    y => y.Should().Be("2"),
                    y => y.Should().Be("3")
                )
            );
    }
}

// Could potentially add tests for int64 etc...
[DynamoDbDocument]
public partial class Int32SetClass
{
    [DynamoDBProperty]
    public HashSet<int>? IntSet { get; set; }
}

[DynamoDbDocument]
public partial class StringSetClass
{
    [DynamoDBProperty]
    public HashSet<string>? StringSet { get; set; }
}