using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Models;


[DynamoDBMarshaller(EntityType = typeof(PersonEntity))]
public partial class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;

    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public Address Address { get; set; } = null!;
}
