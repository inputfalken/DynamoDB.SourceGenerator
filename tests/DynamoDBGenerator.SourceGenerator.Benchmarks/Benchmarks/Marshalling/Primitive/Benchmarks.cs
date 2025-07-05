using System;
using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Primitive;


[DynamoDBMarshaller(EntityType = typeof(Container<Boolean>))]
public partial class System_Bool() : SG_BenchMarker<Container<Boolean>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<Char>))]
public partial class System_Char() : SG_BenchMarker<Container<Char>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<Int32>))]
public partial class System_Int32() : SG_BenchMarker<Container<Int32>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<Int64>))]
public partial class System_Int64() : SG_BenchMarker<Container<Int64>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<String>))]
public partial class System_String() : SG_BenchMarker<Container<String>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<UInt32>))]
public partial class System_UInt32() : SG_BenchMarker<Container<UInt32>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<UInt64>))]
public partial class System_UInt64() : SG_BenchMarker<Container<UInt64>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<Guid>))]
public partial class System_Guid() : SG_BenchMarker<Container<Guid>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
public partial class System_Enum() : SG_BenchMarker<Container<DayOfWeek>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<Double>))]
public partial class System_Double() : SG_BenchMarker<Container<Double>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);