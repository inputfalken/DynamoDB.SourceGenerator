namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class LookUpTests
{
    [Fact]
    public void BuildAttributeValues_EmptyLookup_IsIncluded()
    {
        var @class = new LookUpClass()
        {
            Lookup = Enumerable.Empty<string>().ToLookup(x => x, _ => 1)
        };

        var result = @class.BuildAttributeValues();

        result.Should()
            .ContainKey(nameof(LookUpClass.Lookup))
            .And
            .SatisfyRespectively(x => x.Value.M.Should().BeEmpty());
    }

    [Fact]
    public void BuildAttributeValues_LookupWithValues_IsIncluded()
    {
        var @class = new LookUpClass
        {
            Lookup = new List<KeyValuePair<string, int>>
            {
                new("first", 1),
                new("first", 2),
                new("second", 3),
                new("second", 4)
            }.ToLookup(x => x.Key, x => x.Value)
        };

        var result = @class.BuildAttributeValues();

        result.Should()
            .ContainKey(nameof(LookUpClass.Lookup))
            .And
            .SatisfyRespectively(x =>
            {
                x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be("first");
                        y.Value.L.Should().SatisfyRespectively(z => { z.N.Should().Be("1"); },
                            z => { z.N.Should().Be("2"); });
                    },
                    y =>
                    {
                        y.Key.Should().Be("second");
                        y.Value.L.Should().SatisfyRespectively(z => { z.N.Should().Be("3"); },
                            z => { z.N.Should().Be("4"); });
                    });
            });
    }
}

[AttributeValueGenerator]
public partial class LookUpClass
{
    [DynamoDBProperty]
    public ILookup<string, int>? Lookup { get; set; }
}