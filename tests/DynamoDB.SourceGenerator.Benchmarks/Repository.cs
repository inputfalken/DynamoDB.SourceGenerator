using DynamoDB.SourceGenerator.Benchmarks.Models;
using DynamoDBGenerator.Attributes;

namespace DynamoDB.SourceGenerator.Benchmarks;

[DynamoDBMarshaller(typeof(PersonEntity))]
[DynamoDBMarshaller(typeof(Test))]
public partial class Repository
{
}