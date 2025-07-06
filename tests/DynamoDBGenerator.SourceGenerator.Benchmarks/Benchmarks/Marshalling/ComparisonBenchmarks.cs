using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions.Types;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling;

[DynamoDBMarshaller(EntityType = typeof(Person))]
public partial class ComparisonBenchmarks
{
    private readonly AWSComparisonBenchmarkHelper<Person, Person, PersonNames, PersonValues> _marshaller =
        PersonMarshaller.ToAwsComparisonHelper();

    [Benchmark]
    public Person Unmarshall_Person_DTO() => _marshaller.Unmarshall();

    [Benchmark]
    public Person Amazon_Unmarshall_Person_DTO() => _marshaller.Unmarshall_AWS();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Person_DTO() => _marshaller.Marshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Amazon_Marshall_Person_DTO() => _marshaller.Marshall_AWS();
}