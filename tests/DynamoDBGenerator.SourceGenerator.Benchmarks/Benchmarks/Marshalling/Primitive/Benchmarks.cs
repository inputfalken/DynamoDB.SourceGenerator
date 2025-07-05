using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Primitive;

[DynamoDBMarshaller(EntityType = typeof(Container<System.Boolean>))]
public partial class Bool() : SG_VS_AWS_Benchmarker<Container<System.Boolean>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.Char>))]
public partial class Char() : SG_VS_AWS_Benchmarker<Container<System.Char>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.Int32>))]
public partial class Int32() : SG_VS_AWS_Benchmarker<Container<System.Int32>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.Int64>))]
public partial class Int64() : SG_VS_AWS_Benchmarker<Container<System.Int64>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.String>))]
public partial class String() : SG_VS_AWS_Benchmarker<Container<System.String>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.UInt32>))]
public partial class UInt32() : SG_VS_AWS_Benchmarker<Container<System.UInt32>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.UInt64>))]
public partial class UInt64() : SG_VS_AWS_Benchmarker<Container<System.UInt64>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.Guid>))]
public partial class Guid() : SG_VS_AWS_Benchmarker<Container<System.Guid>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
public partial class Enum() : SG_VS_AWS_Benchmarker<Container<DayOfWeek>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);

[DynamoDBMarshaller(EntityType = typeof(Container<System.Double>))]
public partial class Double() : SG_VS_AWS_Benchmarker<Container<System.Double>>(
    ContainerMarshaller.Marshall,
    ContainerMarshaller.Unmarshall
);