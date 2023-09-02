using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBMarshaller(typeof(DictionaryClass))]
public partial class DictionaryTests
{
    [Fact]
    public void Deserialize_CustomValuedDictionary_ShouldContainCorrectKeyValues()
    {
        DictionaryClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DictionaryClass.CustomValuedDictionary), new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {
                                "Test", new AttributeValue
                                {
                                    M = new Dictionary<string, AttributeValue>
                                    {

                                        {"Name", new AttributeValue {S = "John"}},
                                        {"SomethingElse", new AttributeValue {S = "FooBar"}}
                                    }
                                }
                            },
                            {
                                "Test2", new AttributeValue
                                {
                                    M = new Dictionary<string, AttributeValue>
                                    {

                                        {"Name", new AttributeValue {S = "John2"}},
                                        {"SomethingElse", new AttributeValue {S = "FooBar2"}}
                                    }
                                }
                            }
                        }
                    }
                }
            })
            .CustomValuedDictionary
            .Should()
            .BeOfType<Dictionary<string, DictionaryClass.CustomClass>>()
            .Which
            .Should()
            .SatisfyRespectively(x =>
                {
                    x.Key.Should().Be("Test");
                    x.Value.Name.Should().Be("John");
                    x.Value.SomethingElse.Should().Be("FooBar");
                },
                x =>
                {
                    x.Key.Should().Be("Test2");
                    x.Value.Name.Should().Be("John2");
                    x.Value.SomethingElse.Should().Be("FooBar2");
                }
            );
    }
    [Fact]
    public void Deserialize_Dictionary_ShouldContainCorrectKeyValues()
    {
        DictionaryClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DictionaryClass.Dictionary), new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {"Two", new AttributeValue {N = "2"}},
                            {"One", new AttributeValue {N = "1"}}
                        }
                    }
                }
            })
            .Dictionary
            .Should()
            .BeOfType<Dictionary<string, int>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("Two", 2)), x => x.Should().Be(new KeyValuePair<string, int>("One", 1)));
    }

    [Fact]
    public void Deserialize_IReadOnlyDictionary_ShouldBeDictionaryAndContainCorrectKeyValues()
    {
        DictionaryClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DictionaryClass.ReadOnlyDictionaryInterface), new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {"Two", new AttributeValue {N = "2"}},
                            {"One", new AttributeValue {N = "1"}}
                        }
                    }
                }
            })
            .ReadOnlyDictionaryInterface
            .Should()
            .BeOfType<Dictionary<string, int>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("Two", 2)), x => x.Should().Be(new KeyValuePair<string, int>("One", 1)));
    }
    [Fact]
    public void Deserialize_IDictionary_ShouldBeDictionaryAndContainCorrectKeyValues()
    {
        DictionaryClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DictionaryClass.DictionaryInterface), new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {"Two", new AttributeValue {N = "2"}},
                            {"One", new AttributeValue {N = "1"}}
                        }
                    }
                }
            })
            .DictionaryInterface
            .Should()
            .BeOfType<Dictionary<string, int>>()
            .Which
            .Should()
            .SatisfyRespectively(x => x.Should().Be(new KeyValuePair<string, int>("Two", 2)), x => x.Should().Be(new KeyValuePair<string, int>("One", 1)));
    }

}

public class DictionaryClass
{
    [DynamoDBProperty]
    public Dictionary<string, int>? Dictionary { get; set; }

    [DynamoDBProperty]
    public IReadOnlyDictionary<string, int>? ReadOnlyDictionaryInterface { get; set; }

    [DynamoDBProperty]
    public IDictionary<string, int>? DictionaryInterface { get; set; }

    public Dictionary<string, CustomClass>? CustomValuedDictionary { get; set; }

    public class CustomClass
    {
        public string? Name { get; set; }
        public string? SomethingElse { get; set; }
    }
}