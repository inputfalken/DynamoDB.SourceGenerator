namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Generics;

[DynamoDBGenerator.DynamoDBDocument(typeof(DictionaryClass))]
public partial class DictionaryTests
{

    [Fact]
    public void Serialize_DictionaryWithValues_IsIncluded()
    {
        var @class = new DictionaryClass
        {
            DictionaryImplementation = new Dictionary<string, int>
            {
                {"two", 2},
                {"one", 1}
            }
        };

        DictionaryClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DictionaryClass.DictionaryImplementation))
            .And
            .SatisfyRespectively(x =>
            {
                x.Value.M.Should().SatisfyRespectively(y =>
                {
                    ((string)y.Key).Should().Be("two");
                    ((string)y.Value.N).Should().Be("2");
                }, y =>
                {

                    ((string)y.Key).Should().Be("one");
                    ((string)y.Value.N).Should().Be("1");
                });
            });
    }
    [Fact]
    public void Serialize_EmptyDictionary_IsIncluded()
    {
        var @class = new DictionaryClass
        {
            DictionaryImplementation = new Dictionary<string, int>()
        };

        DictionaryClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DictionaryClass.DictionaryImplementation))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }
}

public class DictionaryClass
{
    [DynamoDBProperty]
    public Dictionary<string, int>? DictionaryImplementation { get; set; }

    [DynamoDBProperty]
    public IReadOnlyDictionary<string, int>? ReadOnlyDictionary { get; set; }

    [DynamoDBProperty]
    public IDictionary<string, int>? DictionaryInterface { get; set; }
}
