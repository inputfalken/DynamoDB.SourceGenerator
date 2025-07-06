using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

public record Person
{
    [DynamoDBHashKey]
    public required string Id { get; set; }

    public required List<string> Strings { get; set; }
    public required List<int> Numbers { get; set; }
    public required List<Person> Neighbours { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required DateTime BirthDate { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required DateTime InsertedAt { get; set; }
    public required DateTime? DeletedAt { get; set; }
    public required Address Address { get; set; }
}