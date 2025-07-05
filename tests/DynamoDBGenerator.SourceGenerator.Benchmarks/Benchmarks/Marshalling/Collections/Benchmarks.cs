using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Collections;

[DynamoDBMarshaller(EntityType = typeof(Container<List<string>>))]
public partial class StringList() : SG_BenchMarker<Container<List<string>>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<Dictionary<string, int>>))]
public partial class StringIntDictionary() : SG_BenchMarker<Container<Dictionary<string, int>>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<string>>))]
public partial class StringHashSet() : SG_BenchMarker<Container<HashSet<string>>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<int>>))]
public partial class IntHashSet() : SG_BenchMarker<Container<HashSet<int>>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<List<KeyValuePair<string, int>>>))]
public partial class KeyValuePairList() : SG_BenchMarker<Container<List<KeyValuePair<string, int>>>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);