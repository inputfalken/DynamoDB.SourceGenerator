using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Options;

namespace Configuration;

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.Name)]
[DynamoDBMarshaller]
public partial record EnumBehaviour([property: DynamoDBHashKey] string Id, DayOfWeek MyEnum);