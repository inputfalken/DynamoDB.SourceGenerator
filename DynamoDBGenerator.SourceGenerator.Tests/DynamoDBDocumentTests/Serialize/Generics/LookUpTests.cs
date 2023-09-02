namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBMarshaller(typeof(LookUpClass))]
public partial class LookUpTests
{
    [Fact]
    public void Serialize_EmptyLookup_IsIncluded()
    {
        var @class = new LookUpClass
        {
            Lookup = Enumerable.Empty<string>().ToLookup(x => x, _ => 1)
        };

        LookUpClassMarshaller
            .Marshall(@class)
            .Should()
            .ContainKey(nameof(LookUpClass.Lookup))
            .And
            .SatisfyRespectively(x => x.Value.M.Should().BeEmpty());
    }

    [Fact]
    public void Serialize_LookupWithValues_IsIncluded()
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

        LookUpClassMarshaller
            .Marshall(@class)
            .Should()
            .ContainKey(nameof(LookUpClass.Lookup))
            .And
            .SatisfyRespectively(x =>
            {
                x.Value.M.Should().SatisfyRespectively(y =>
                    {
                        y.Key.Should().Be("first");
                        y.Value.L.Should().SatisfyRespectively(z => { ((string)z.N).Should().Be("1"); },
                            z => { ((string)z.N).Should().Be("2"); });
                    },
                    y =>
                    {
                        ((string)y.Key).Should().Be("second");
                        y.Value.L.Should().SatisfyRespectively(z => { ((string)z.N).Should().Be("3"); },
                            z => { ((string)z.N).Should().Be("4"); });
                    });
            });
    }
}

public class LookUpClass
{
    [DynamoDBProperty]
    public ILookup<string, int>? Lookup { get; set; }
}
