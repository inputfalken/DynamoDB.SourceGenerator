using System.Collections.Generic;

namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class SetTests
{
    [Fact]
    public void BuildAttributeValues_EmptySet_IsIncluded()
    {
        var @class = new SetClass
        {
            Set = new HashSet<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(EnumerableClass.Strings))
            .And
            .ContainSingle(x => x.Value.SS.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_NullSet_IsSkipped()
    {
        var @class = new SetClass()
        {
            Set = null
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_SetWithValues_IsIncluded()
    {
        var @class = new SetClass
        {
            Set = new HashSet<string>(new[] {"1", "2", "3"})
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(EnumerableClass.Strings))
            .And
            .ContainSingle(x => x.Value.SS.Count == 3)
            .And
            .AllSatisfy(x => x.Value.SS
                .Should()
                .SatisfyRespectively(
                    y => y.Should().Be("1"),
                    y => y.Should().Be("2"),
                    y => y.Should().Be("3")
                )
            );
    }
}

[AttributeValueGenerator]
public partial class SetClass
{
    [DynamoDBProperty] public HashSet<string>? Set = new();
}