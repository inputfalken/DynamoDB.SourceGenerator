using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling;

[DynamoDBMarshaller(EntityType = typeof(Container<Dictionary<string, int>>), AccessName = "DictionaryMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<string>>), AccessName = "StringHashSetMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<List<string>>), AccessName = "ListMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<int>>), AccessName = "IntHashSetMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<List<KeyValuePair<string, int>>>), AccessName = "KeyValuePairListMarshaller")]
public partial class CollectionBenchmarks
{
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<Dictionary<string, int>>, Container<Dictionary<string, int>>, ContainerDictionarystringintNames, ContainerDictionarystringintValues> _dictionary = DictionaryMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<HashSet<string>>, Container<HashSet<string>>, ContainerHashSetstringNames, ContainerHashSetstringValues> _stringHashSet = StringHashSetMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<List<string>>, Container<List<string>>, ContainerListstringNames, ContainerListstringValues> _stringList = ListMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<HashSet<int>>, Container<HashSet<int>>, ContainerHashSetintNames, ContainerHashSetintValues> _intHashSet = IntHashSetMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<List<KeyValuePair<string, int>>>, Container<List<KeyValuePair<string, int>>>, ContainerListKeyValuePairstringintNames, ContainerListKeyValuePairstringintValues> _keyValuePairList = KeyValuePairListMarshaller.ToBenchmarkHelper();

    [Benchmark]
    public Container<Dictionary<string, int>> Unmarshall_Dictionary() => _dictionary.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Dictionary() => _dictionary.Marshall();

    [Benchmark]
    public Container<HashSet<string>> Unmarshall_StringHashSet() => _stringHashSet.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_StringHashSet() => _stringHashSet.Marshall();
    
    [Benchmark]
    public Container<List<string>> Unmarshall_StringList() => _stringList.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_StringList() => _stringList.Marshall();
    
    [Benchmark]
    public Container<HashSet<int>> Unmarshall_IntHashSet() => _intHashSet.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_IntHashSet() => _intHashSet.Marshall();
    
    [Benchmark]
    public Container<List<KeyValuePair<string,int>>> Unmarshall_KeyValuePairList() => _keyValuePairList.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_KeyValuePairList() => _keyValuePairList.Marshall();
}