using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBMarshaller(EntityType = typeof(DictionaryClass))]
public partial class DictionaryTests
{
    [Fact]
    public void Serialize_DictionaryWithValues_IsIncluded()
    {
        var @class = new DictionaryClass
        {
            DictionaryImplementation = new Dictionary<string, int>
            {
                { "two", 2 },
                { "one", 1 }
            }
        };

        DictionaryClassMarshaller
            .Marshall(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DictionaryClass.DictionaryImplementation))
            .And
            .SatisfyRespectively(x =>
            {
                x.Value.M.Should().SatisfyRespectively(y =>
                {
                    y.Key.Should().Be("two");
                    y.Value.N.Should().Be("2");
                }, y =>
                {
                    y.Key.Should().Be("one");
                    y.Value.N.Should().Be("1");
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

        var result = DictionaryClassMarshaller.Marshall(@class);
        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DictionaryClass.DictionaryImplementation));

        result[nameof(DictionaryClass.DictionaryImplementation)].M.Should().BeEmpty();

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