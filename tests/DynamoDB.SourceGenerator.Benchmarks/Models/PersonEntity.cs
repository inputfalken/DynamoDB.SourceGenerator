using Amazon.DynamoDBv2.DataModel;
namespace DynamoDB.SourceGenerator.Benchmarks.Models;

public class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public Address Address { get; set; }
}