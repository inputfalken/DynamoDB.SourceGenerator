namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBDocument(typeof(ListClass))]
public partial class ListTests
{

    [Fact]
    public void Serialize_CollectionWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            Collection = new[] {"1", "2", "3"}
        };

        ListClassDocument
            .Serialize(@class)
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
                    y => ((string)y.S).Should().Be("3")
                )
            );
    }

    [Fact]
    public void Serialize_EmptyCollection_IsIncluded()
    {
        var @class = new ListClass
        {
            Collection = new List<string>()
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Collection))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void Serialize_EmptyEnumerable_IsIncluded()
    {
        var @class = new ListClass
        {
            Enumerable = Enumerable.Empty<string>()
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Enumerable))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void Serialize_EmptyArray_IsIncluded()
    {
        var @class = new ListClass
        {
            Array = Array.Empty<string>()
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Array))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }
    [Fact]
    public void Serialize_ArrayWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            Array = new[] {"1", "2", "3"}
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.Array))
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
    public void Serialize_EmptyList_IsIncluded()
    {
        var @class = new ListClass
        {
            List = new List<string>()
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.List))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }
    [Fact]
    public void Serialize_EmptyReadOnlyList_IsIncluded()
    {
        var @class = new ListClass
        {
            ReadOnlyList = Array.Empty<string>()
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ListClass.ReadOnlyList))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void Serialize_EnumerableWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            Enumerable = new[] {"1", "2", "3"}
        };

        ListClassDocument
            .Serialize(@class)
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
                    y => ((string)y.S).Should().Be("3")
                )
            );
    }

    [Fact]
    public void Serialize_KeyValuePairListWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            KeyValuePairs = new[] {new KeyValuePair<string, int>("2", 1), new KeyValuePair<string, int>("1", 1)}
        };

        ListClassDocument
            .Serialize(@class)
            .Should()
            .AllSatisfy(x =>
            {
                x.Key.Should().Be(nameof(ListClass.KeyValuePairs));
                x.Value.L.Should().SatisfyRespectively(
                    y => y.M.Should().SatisfyRespectively(
                        z =>
                        {
                            z.Key.Should().Be("Key");
                            ((string)z.Value.S).Should().Be("2");
                        },
                        z =>
                        {
                            ((string)z.Key).Should().Be("Value");
                            ((string)z.Value.N).Should().Be("1");
                        }
                    ),
                    y =>
                    {
                        y.M.Should().SatisfyRespectively(
                            z =>
                            {
                                ((string)z.Key).Should().Be("Key");
                                ((string)z.Value.S).Should().Be("1");
                            },
                            z =>
                            {
                                ((string)z.Key).Should().Be("Value");
                                ((string)z.Value.N).Should().Be("1");
                            }
                        );
                    }
                );
            });
    }

    [Fact]
    public void Serialize_ListWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            List = new[] {"1", "2", "3"}
        };

        ListClassDocument
            .Serialize(@class)
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
    public void Serialize_ReadOnlyListWithValues_IsIncluded()
    {
        var @class = new ListClass
        {
            ReadOnlyList = new[] {"1", "2", "3"}
        };

        ListClassDocument
            .Serialize(@class)
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
                    y => ((string)y.S).Should().Be("3")
                )
            );
    }
}

public class ListClass
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
    public IReadOnlyList<KeyValuePair<string, int>>? KeyValuePairs { get; set; }
    
    public string[]? Array { get; set; }
}
