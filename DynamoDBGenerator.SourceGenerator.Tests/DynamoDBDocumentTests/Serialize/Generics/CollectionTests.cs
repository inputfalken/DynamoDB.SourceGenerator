namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBDocument(typeof(CollectionClass))]
public partial class CollectionTests
{

    [Fact]
    public void Serialize_CollectionWithValues_IsIncluded()
    {
        var @class = new CollectionClass
        {
            Collection = new[] {"1", "2", "3"}
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.Collection))
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
        var @class = new CollectionClass
        {
            Collection = new List<string>()
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.Collection))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void Serialize_EmptyEnumerable_IsIncluded()
    {
        var @class = new CollectionClass
        {
            Enumerable = Enumerable.Empty<string>()
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.Enumerable))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void Serialize_EmptyArray_IsIncluded()
    {
        var @class = new CollectionClass
        {
            Array = Array.Empty<string>()
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.Array))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }
    [Fact]
    public void Serialize_ArrayWithValues_IsIncluded()
    {
        var @class = new CollectionClass
        {
            Array = new[] {"1", "2", "3"}
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.Array))
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
        var @class = new CollectionClass
        {
            List = new List<string>()
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.List))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }
    [Fact]
    public void Serialize_EmptyReadOnlyList_IsIncluded()
    {
        var @class = new CollectionClass
        {
            ReadOnlyList = Array.Empty<string>()
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.ReadOnlyList))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void Serialize_EnumerableWithValues_IsIncluded()
    {
        var @class = new CollectionClass
        {
            Enumerable = new[] {"1", "2", "3"}
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.Enumerable))
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
        var @class = new CollectionClass
        {
            KeyValuePairs = new[] {new KeyValuePair<string, int>("2", 1), new KeyValuePair<string, int>("1", 1)}
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .AllSatisfy(x =>
            {
                x.Key.Should().Be(nameof(CollectionClass.KeyValuePairs));
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
        var @class = new CollectionClass
        {
            List = new[] {"1", "2", "3"}
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.List))
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
        var @class = new CollectionClass
        {
            ReadOnlyList = new[] {"1", "2", "3"}
        };

        CollectionClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(CollectionClass.ReadOnlyList))
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

public class CollectionClass
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
