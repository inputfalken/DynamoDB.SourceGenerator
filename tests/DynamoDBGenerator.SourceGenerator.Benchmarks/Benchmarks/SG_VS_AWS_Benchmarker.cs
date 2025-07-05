using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks;

//public abstract class SG_VS_AWS_Benchmarker<T>(
//    Func<T, Dictionary<string, AttributeValue>> marshaller,
//    Func<Dictionary<string, AttributeValue>, T> unMarshaller
//)
//{
//    private readonly DynamoDBContext _context = new DynamoDBContextBuilder()
//        .WithDynamoDBClient(() => new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
//        .Build();
//
//    private readonly ToDocumentConfig _dynamoDbOperationConfig = new() { Conversion = DynamoDBEntryConversion.V2 };
//
//
//    [Benchmark]
//    public Dictionary<string, AttributeValue> Marshall_AWS_Reflection() => _context
//        .ToDocument(SingleElement, _dynamoDbOperationConfig)
//        .ToAttributeMap(_dynamoDbOperationConfig.Conversion);
//
//    [Benchmark]
//    public T Unmarshall_AWS_Reflection() => _context.FromDocument<T>(Document.FromAttributeMap(AttributeValues));
//}