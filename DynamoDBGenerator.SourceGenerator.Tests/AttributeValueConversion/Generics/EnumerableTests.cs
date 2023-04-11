namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class EnumerableTests
{
    [Fact]
    public void BuildAttributeValues_EmptyEnumerable_IsIncluded()
    {
        var @class = new EnumerableClass
        {
            Strings = Enumerable.Empty<string>()
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(EnumerableClass.Strings))
            .And
            .ContainSingle(x => x.Value.L.Count == 0);
    }

    [Fact]
    public void BuildAttributeValues_NullEnumerable_IsSkipped()
    {
        var @class = new EnumerableClass
        {
            Strings = null
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_EnumerableWithValues_IsIncluded()
    {
        var @class = new EnumerableClass
        {
            Strings = new[] {"1", "2", "3"}
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(EnumerableClass.Strings))
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
}

[AttributeValueGenerator]
public partial class EnumerableClass
{
    [DynamoDBProperty]
    public IEnumerable<string>? Strings { get; set; }
}