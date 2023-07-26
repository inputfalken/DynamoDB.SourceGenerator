using System.Collections;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Generics;

[DynamoDBGenerator.DynamoDBDocument(typeof(GroupingClass))]
public partial class GroupingTests
{
    private class Grouping<TKey, TValue> : IGrouping<TKey, TValue>
    {
        private readonly IEnumerable<TValue> _values;

        public Grouping(TKey key, IEnumerable<TValue> values)
        {
            _values = values;
            Key = key;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TKey Key { get; }
    }

    [Fact]
    public void Serialize_EmptyGrouping_IsIncluded()
    {
        var @class = new GroupingClass
        {
            Grouping = new Grouping<string, int>("A", Array.Empty<int>())
        };

        GroupingClassDocument
            .Serialize(@class)
            .Should()
            .ContainKey(nameof(GroupingClass.Grouping))
            .And
            .SatisfyRespectively(x => x.Value.M.Should().SatisfyRespectively(y =>
            {
                ((string)y.Key).Should().Be("A");
                y.Value.L.Should().BeEmpty();
            }));
    }

    [Fact]
    public void Serialize_GroupingWithValues_IsIncluded()
    {
        var @class = new GroupingClass
        {
            Grouping = new Grouping<string, int>("first", new[] {1, 2})
        };

        GroupingClassDocument
            .Serialize(@class)
            .Should()
            .ContainKey(nameof(GroupingClass.Grouping))
            .And
            .SatisfyRespectively(x =>
            {
                x.Value.M.Should().SatisfyRespectively(y =>
                {
                    ((string)y.Key).Should().Be("first");
                    y.Value.L.Should().SatisfyRespectively(z => { ((string)z.N).Should().Be("1"); },
                        z => { ((string)z.N).Should().Be("2"); });
                });
            });
    }
}

public class GroupingClass
{
    public IGrouping<string, int>? Grouping { get; set; }
}
