using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

[DynamoDBMarshaller(EntityType = typeof(PersonEntity))]
public partial record PersonEntity
{
    public HashSet<string?>? StringSet { get; set; }
    public HashSet<int?>? IntSet { get; set; }
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;

    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime InsertedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Address Address { get; set; } = null!;
}