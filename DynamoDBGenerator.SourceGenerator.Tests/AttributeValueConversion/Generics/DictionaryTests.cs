using System.Collections.Immutable;

namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class DictionaryTests
{
    [Fact]
    public void BuildAttributeValues_EmptyDictionary_IsIncluded()
    {
        var @class = new DictionaryClass
        {
            DictionaryImplementation = new Dictionary<string, int>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DictionaryClass.DictionaryImplementation))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_DictionaryWithValues_IsIncluded()
    {
        var @class = new DictionaryClass
        {
            DictionaryImplementation = new Dictionary<string, int>
            {
                {"two", 2},
                {"one", 1}
            }
        };

        var result = @class.BuildAttributeValues();

        result
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
}

[DynamoDbDocument]
public partial class DictionaryClass
{
    [DynamoDBProperty]
    public Dictionary<string, int>? DictionaryImplementation { get; set; }

    [DynamoDBProperty]
    public IReadOnlyDictionary<string, int>? ReadOnlyDictionary { get; set; }

    [DynamoDBProperty]
    public IDictionary<string, int>? DictionaryInterface { get; set; }
}