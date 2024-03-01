using System.Net.Sockets;
using DynamoDB.SourceGenerator.Benchmarks.Models;
using DynamoDBGenerator.Attributes;

namespace DynamoDB.SourceGenerator.Benchmarks;


[DynamoDBMarshaller(typeof(PersonEntity))]
public static partial class Repository
{
}