using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Temporal;

[DynamoDBMarshaller(EntityType = typeof(Container<System.DateTime>))]
public partial class DateTime() : SG_VS_AWS_Benchmarker<Container<System.DateTime>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.DateTimeOffset>))]
public partial class DateTimeOffset() : SG_BenchMarker<Container<System.DateTimeOffset>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.DateOnly>))]
public partial class DateOnly() : SG_BenchMarker<Container<System.DateOnly>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.TimeOnly>))]
public partial class TimeOnly() : SG_BenchMarker<Container<System.TimeOnly>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);