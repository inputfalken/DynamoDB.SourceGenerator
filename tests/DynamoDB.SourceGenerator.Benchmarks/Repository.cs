using DynamoDB.SourceGenerator.Benchmarks.Models;
using DynamoDBGenerator.Attributes;

namespace DynamoDB.SourceGenerator.Benchmarks;

[DynamoDBMarshaller(typeof(PersonEntity))]
public partial class Repository
{
}