using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;

namespace TypeSupport;

[DynamoDBMarshaller]
public partial record Record([property: DynamoDBHashKey] string Id);

[DynamoDBMarshaller]
public partial class Class
{
    [DynamoDBHashKey]
    public string Id { get; init; }
}

[DynamoDBMarshaller]
public partial struct Struct
{
    [DynamoDBHashKey]
    public string Id { get; init; }
}

[DynamoDBMarshaller]
public readonly partial struct ReadOnlyStruct
{
    [DynamoDBHashKey]
    public string Id { get; init; }
}

[DynamoDBMarshaller]
public readonly partial record struct ReadOnlyRecordStruct([property: DynamoDBHashKey] string Id);