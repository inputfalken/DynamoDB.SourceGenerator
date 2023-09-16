using System;
using AutoFixture;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests;

[DynamoDBMarshaller(typeof(TypeWithPartitionKeyOnly), PropertyName = "PartitionKeyOnly")]
[DynamoDBMarshaller(typeof(TypeWithRangeKeyOnly), PropertyName = "TypeWithRangeOnly")]
[DynamoDBMarshaller(typeof(TypeWithKeys), PropertyName = "TypeWithKeys")]
[DynamoDBMarshaller(typeof(TypeWithoutKeys), PropertyName = "TypeWithoutKeys")]
public partial class DynamoDBKeyMarshallerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void PartitionKey_TypeWithPartitionKeyOnly_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithPartitionKeyOnly>();
        PartitionKeyOnly
            .PartitionKey(type.Id)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(TypeWithPartitionKeyOnly.Id));
                x.Value.S.Should().Be(type.Id);
            });
    }

    [Fact]
    public void RangeKey_TypeWithPartitionKeyOnly_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithPartitionKeyOnly>();
        var act = () => PartitionKeyOnly.RangeKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("abc", null, Skip = "Could make sense to allow this")]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithPartitionKeyOnly_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => PartitionKeyOnly.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PartitionKey_TypeWithRangeKeyOnly_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithRangeKeyOnly>();
        var act = () => TypeWithRangeOnly.PartitionKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RangeKey_TypeWithRangeKeyOnly_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithRangeKeyOnly>();
        TypeWithRangeOnly
            .RangeKey(type.Id)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(type.Id));
                x.Value.S.Should().Be(type.Id);
            });
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc", Skip = "Could make sense to allow this")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithRangeKeyOnly_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => TypeWithRangeOnly.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PartitionKey_TypeWithoutKeys_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithKeys>();
        var act = () => TypeWithoutKeys.PartitionKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RangeKey_TypeWithoutKeys_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithoutKeys>();
        var act = () => TypeWithoutKeys.RangeKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithoutKeys_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => TypeWithoutKeys.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }


    [Fact]
    public void PartitionKey_TypeWithKeys_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithKeys>();
        TypeWithKeys
            .PartitionKey(type.Id)
            .Should().SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(type.Id));
                x.Value.S.Should().Be(type.Id);
            });
    }

    [Fact]
    public void RangeKey_TypeWithKeys_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithKeys>();
        TypeWithKeys.RangeKey(type.RangeKey).Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(type.RangeKey));
            x.Value.S.Should().Be(type.RangeKey);
        });
    }

    [Fact]
    public void Keys_ValidTypeWithKeys_ShouldSucceed()
    {
        var keys = _fixture.Create<TypeWithKeys>();
        TypeWithKeys
            .Keys(keys.Id, keys.RangeKey)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(keys.Id));
            }, x =>
            {

                x.Key.Should().Be(nameof(keys.RangeKey));
            });
    }
    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", 1)]
    [InlineData(1, "abc")]
    [InlineData(1, 2)]
    public void Keys_InvalidTypeWithKeys_ShouldThrow(object partitionKey, object rangeKey)
    {
        var act = () => TypeWithKeys.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(long.MaxValue)]
    [InlineData(double.MaxValue)]
    [InlineData(float.MaxValue)]
    [InlineData(typeof(object))]
    [InlineData('A')]
    public void PartitionKey_InvalidTypes_ShouldThrow(object key)
    {
        var act = () => PartitionKeyOnly.PartitionKey(key);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(long.MaxValue)]
    [InlineData(double.MaxValue)]
    [InlineData(float.MaxValue)]
    [InlineData(typeof(object))]
    [InlineData('A')]
    public void RangeKey_InvalidTypes_ShouldThrow(object key)
    {
        var act = () => PartitionKeyOnly.RangeKey(key);
        act.Should().Throw<InvalidOperationException>();

    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(long.MaxValue)]
    [InlineData(double.MaxValue)]
    [InlineData(float.MaxValue)]
    [InlineData(typeof(object))]
    [InlineData('A')]
    public void Keys_InvalidTypes_ShouldThrow(object key)
    {
        var act = () => PartitionKeyOnly.Keys(key, key);
        act.Should().Throw<InvalidOperationException>();
    }
}

public class TypeWithPartitionKeyOnly
{
    [DynamoDBHashKey]
    public string Id { get; set; }
}

public class TypeWithRangeKeyOnly
{
    [DynamoDBRangeKey]
    public string Id { get; set; }
}

public class TypeWithKeys
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBRangeKey]
    public string RangeKey { get; set; }
}

public class TypeWithoutKeys
{
    public string Id { get; set; }
}