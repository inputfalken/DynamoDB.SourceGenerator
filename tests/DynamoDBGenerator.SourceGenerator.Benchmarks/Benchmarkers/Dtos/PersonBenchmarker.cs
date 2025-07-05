using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarkers.Dtos;

//[SimpleJob(RuntimeMoniker.Net80)]
//[MemoryDiagnoser]
//public class PersonBenchmarker() : MarshalBenchmarker<PersonEntity>(
//    PersonEntity.PersonEntityMarshaller.Marshall,
//    PersonEntity.PersonEntityMarshaller.Unmarshall
//);