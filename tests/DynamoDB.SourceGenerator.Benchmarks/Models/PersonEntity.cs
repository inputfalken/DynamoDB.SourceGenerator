using Amazon.DynamoDBv2.DataModel;
namespace DynamoDB.SourceGenerator.Benchmarks.Models;

public class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;

    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public Address Address { get; set; } = null!;
}
