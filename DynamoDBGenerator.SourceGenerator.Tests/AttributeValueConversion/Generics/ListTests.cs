namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class ListTests
{
    [Fact]
    public void BuildAttributeValues_EmptyReadOnlyList_IsIncluded()
    {
        var @class = new ListClass
        {
            ReadOnlyList = Array.Empty<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.ReadOnlyList))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_ReadOnlyListWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            ReadOnlyList = new[] {"1", "2", "3"}
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.ReadOnlyList))
            .And
            .AllSatisfy(x => x.Value.L
                .Should()
                .SatisfyRespectively(
                    y => y.S.Should().Be("1"),
                    y => y.S.Should().Be("2"),
                    y => y.S.Should().Be("3")
                )
            );
    }

    [Fact]
    public void BuildAttributeValues_EmptyList_IsIncluded()
    {
        var @class = new ListClass
        {
            List = new List<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.List))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_ListWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            List = new[] {"1", "2", "3"}
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.List))
            .And
            .AllSatisfy(x => x.Value.L
                .Should()
                .SatisfyRespectively(
                    y => y.S.Should().Be("1"),
                    y => y.S.Should().Be("2"),
                    y => y.S.Should().Be("3")
                )
            );
    }

    [Fact]
    public void BuildAttributeValues_EmptyCollection_IsIncluded()
    {
        var @class = new ListClass
        {
            Collection = new List<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Collection))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_CollectionWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            Collection = new[] {"1", "2", "3"}
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Collection))
            .And
            .AllSatisfy(x => x.Value.L
                .Should()
                .SatisfyRespectively(
                    y => y.S.Should().Be("1"),
                    y => y.S.Should().Be("2"),
                    y => y.S.Should().Be("3")
                )
            );
    }

    [Fact]
    public void BuildAttributeValues_EmptyEnumerable_IsIncluded()
    {
        var @class = new ListClass
        {
            Enumerable = Enumerable.Empty<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Enumerable))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_EnumerableWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            Enumerable = new[] {"1", "2", "3"}
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Enumerable))
            .And
            .AllSatisfy(x => x.Value.L
                .Should()
                .SatisfyRespectively(
                    y => y.S.Should().Be("1"),
                    y => y.S.Should().Be("2"),
                    y => y.S.Should().Be("3")
                )
            );
    }

    [Fact]
    public void BuildAttributeValues_KeyValuePairListWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            KeyValuePairs = new[] {new KeyValuePair<string, int>("2", 1), new KeyValuePair<string, int>("1", 1)}
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.KeyValuePairs))
            .And
            .AllSatisfy(x => x.Value.L
                .Should()
                .SatisfyRespectively(
                    y => y.M.Should().SatisfyRespectively(z =>
                    {
                        z.Key.Should().Be("2");
                        z.Value.N.Should().Be("1");
                    }),
                    y => y.M.Should().SatisfyRespectively(z =>
                    {
                        
                        z.Key.Should().Be("1");
                        z.Value.N.Should().Be("1");
                    })
                )
            );
    }
}

[AttributeValueGenerator]
public partial class ListClass
{
    [DynamoDBProperty]
    public IEnumerable<string>? Enumerable { get; set; }

    [DynamoDBProperty]
    public ICollection<string>? Collection { get; set; }

    [DynamoDBProperty]
    public IList<string>? List { get; set; }

    [DynamoDBProperty]
    public IReadOnlyList<string>? ReadOnlyList { get; set; }

    [DynamoDBProperty]
    public IReadOnlyList<KeyValuePair<string, int>> KeyValuePairs { get; set; }
}