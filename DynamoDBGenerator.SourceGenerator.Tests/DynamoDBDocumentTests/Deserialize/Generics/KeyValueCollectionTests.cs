using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(typeof(KeyValueCollectionClass))]
public partial class KeyValueCollectionTests
{

    [Fact]
    public void Deserialize_ICollection_ShouldBeListWithCorrectElements()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.CollectionInterface), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .CollectionInterface
            .Should()
            .BeOfType<List<KeyValuePair<string, int>>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }

    [Fact]
    public void Deserialize_IReadOnlyCollection_ShouldBeArrayWithCorrectElements()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.ReadOnlyCollectionInterface), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .ReadOnlyCollectionInterface
            .Should()
            .BeOfType<KeyValuePair<string, int>[]>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }

    [Fact]
    public void Deserialize_IList_ShouldBeListWithCorrectElements()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.ListInterface), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .ListInterface
            .Should()
            .BeOfType<List<KeyValuePair<string, int>>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }

    [Fact]
    public void Deserialize_IReadOnlyList_ShouldBeArrayWithCorrectElements()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.ReadOnlyListInterface), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .ReadOnlyListInterface
            .Should()
            .BeOfType<KeyValuePair<string, int>[]>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }

    [Fact]
    public void Deserialize_List_ShouldBeListWithCorrectElements()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.List), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .List
            .Should()
            .BeOfType<List<KeyValuePair<string, int>>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }

    [Fact]
    public void Deserialize_Array_ShouldBeArrayWithCorrectElements()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.Array), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .Array
            .Should()
            .BeOfType<KeyValuePair<string, int>[]>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }
    
    [Fact]
    public void Deserialize_IEnumerable_ShouldContainCorrectValues()
    {
        KeyValueCollectionClassMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(KeyValueCollectionClass.EnumerableInterface), new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new()
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "ABC"}},
                                    {"Value", new AttributeValue {N = "1"}}
                                }
                            },
                            new()
                            {

                                M = new Dictionary<string, AttributeValue>
                                {
                                    {"Key", new AttributeValue {S = "Foo"}},
                                    {"Value", new AttributeValue {N = "2"}}
                                }
                            }
                        }
                    }
                }
            })
            .EnumerableInterface
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("ABC", 1)), x => x.Should().Be(new KeyValuePair<string, int>("Foo", 2)));
    }

}

public class KeyValueCollectionClass
{
    public IEnumerable<KeyValuePair<string, int>>? EnumerableInterface { get; set; }
    public List<KeyValuePair<string, int>>? List { get; set; }
    public KeyValuePair<string, int>[]? Array { get; set; }
    public IList<KeyValuePair<string, int>>? ListInterface { get; set; }
    public IReadOnlyList<KeyValuePair<string, int>>? ReadOnlyListInterface { get; set; }
    public ICollection<KeyValuePair<string, int>>? CollectionInterface { get; set; }
    public IReadOnlyCollection<KeyValuePair<string, int>>? ReadOnlyCollectionInterface { get; set; }

}