using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks;

public readonly struct Container<T>
{
    public required T Value { get; init; }
}