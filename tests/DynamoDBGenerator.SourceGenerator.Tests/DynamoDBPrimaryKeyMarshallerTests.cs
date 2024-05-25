using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests;

[DynamoDBMarshaller(EntityType = typeof(TypeWithPartitionKeyOnly), AccessName = "PartitionKeyOnly")]
[DynamoDBMarshaller(EntityType = typeof(TypeWithRangeKeyOnly), AccessName = "TypeWithRangeOnly")]
[DynamoDBMarshaller(EntityType = typeof(TypeWithKeys), AccessName = "TypeWithKeys")]
[DynamoDBMarshaller(EntityType = typeof(TypeWithoutKeys), AccessName = "TypeWithoutKeys")]
public partial class DynamoDBPrimaryKeyMarshallerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void PartitionKey_TypeWithPartitionKeyOnly_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithPartitionKeyOnly>();
        PartitionKeyOnly.PrimaryKeyMarshaller
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
        var act = () => PartitionKeyOnly.PrimaryKeyMarshaller.RangeKey(type.Id);
        NoCorrespondingAttributeAssertion(type.Id, act);
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithPartitionKeyOnly_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => PartitionKeyOnly.PrimaryKeyMarshaller.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PartitionKey_TypeWithRangeKeyOnly_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithRangeKeyOnly>();
        var act = () => TypeWithRangeOnly.PrimaryKeyMarshaller.PartitionKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RangeKey_TypeWithRangeKeyOnly_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithRangeKeyOnly>();
        var act = () => TypeWithRangeOnly.PrimaryKeyMarshaller.RangeKey(type.Id);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithRangeKeyOnly_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => TypeWithRangeOnly.PrimaryKeyMarshaller.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PartitionKey_TypeWithoutKeys_ShouldThrow()
    {
        var type = _fixture.Create<TypeWithKeys>();
        var act = () => TypeWithoutKeys.PrimaryKeyMarshaller.PartitionKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RangeKey_TypeWithoutKeys_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithoutKeys>();
        var act = () => TypeWithoutKeys.PrimaryKeyMarshaller.RangeKey(type.Id);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData(null, "abc")]
    [InlineData(null, null)]
    [InlineData("abc", "dfg")]
    public void Keys_TypeWithoutKeys_ShouldThrow(string partitionKey, string rangeKey)
    {
        var act = () => TypeWithoutKeys.PrimaryKeyMarshaller.Keys(partitionKey, rangeKey);
        act.Should().Throw<InvalidOperationException>();
    }


    [Fact]
    public void PartitionKey_TypeWithKeys_ShouldSucceed()
    {
        var type = _fixture.Create<TypeWithKeys>();
        TypeWithKeys
            .PrimaryKeyMarshaller
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
        TypeWithKeys.PrimaryKeyMarshaller.RangeKey(type.RangeKey).Should().SatisfyRespectively(x =>
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
            .PrimaryKeyMarshaller
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
    public void Keys_InvalidTypesWithKeys_ShouldThrow(object partitionKey, object rangeKey)
    {
        var act = () => TypeWithKeys.PrimaryKeyMarshaller.Keys(partitionKey, rangeKey);
        act.Should().Throw<DynamoDBMarshallingException>();
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
        var act = () => PartitionKeyOnly.PrimaryKeyMarshaller.PartitionKey(key);
        act.Should().Throw<DynamoDBMarshallingException>();
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
        var act = () => PartitionKeyOnly.PrimaryKeyMarshaller.RangeKey(key);
        NoCorrespondingAttributeAssertion(key, act);
    }
    private static void NoCorrespondingAttributeAssertion(object key, Func<Dictionary<string, AttributeValue>> act)
    {
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"Value '{key}' from argument 'rangeKey' was provided but there's no corresponding DynamoDBKeyAttribute.");
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
        var act = () => PartitionKeyOnly.PrimaryKeyMarshaller.Keys(key, key);
        act.Should().Throw<DynamoDBMarshallingException>();
    }
}

public class TypeWithPartitionKeyOnly
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;
}

public class TypeWithRangeKeyOnly
{
    [DynamoDBRangeKey]
    public string Id { get; set; } = null!;
}

public class TypeWithKeys
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;

    [DynamoDBRangeKey]
    public string RangeKey { get; set; } = null!;
}

public class TypeWithoutKeys
{
    public string Id { get; set; } = null!;
}
