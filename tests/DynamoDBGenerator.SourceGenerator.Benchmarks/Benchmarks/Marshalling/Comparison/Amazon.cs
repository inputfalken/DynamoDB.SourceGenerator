using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions.Types;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Comparison;

[DynamoDBMarshaller(EntityType = typeof(Person))]
public partial class Amazon
{
    private readonly AWSComparisonBenchmarkHelper<Person, Person, PersonNames, PersonValues> _marshaller = PersonMarshaller.ToAwsComparisonHelper();

    [Benchmark(Baseline = true)]
    public Person Unmarshall_Person_DTO() => _marshaller.Unmarshall();

    [Benchmark(Baseline = true)]
    public Dictionary<string, AttributeValue> Marshall_Person_DTO() => _marshaller.Marshall();

    [Benchmark]
    public Person AWS_Unmarshall_Person_DTO() => _marshaller.Unmarshall_AWS();

    [Benchmark]
    public Dictionary<string, AttributeValue> AWS_Marshall_Person_DTO() => _marshaller.Marshall_AWS();
}