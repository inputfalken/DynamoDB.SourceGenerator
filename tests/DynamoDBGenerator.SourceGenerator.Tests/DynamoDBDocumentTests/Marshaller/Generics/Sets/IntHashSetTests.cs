using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(SetDto))]
public partial class IntHashSetTests : SetAsserter<HashSet<int>, int>
{

    public IntHashSetTests() : base(new[] {2, 3, 4, 5}, x => new HashSet<int>(x))
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(SetDto element)
    {
        return SetDtoMarshaller.Marshall(element);
    }
    protected override SetDto UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return SetDtoMarshaller.Unmarshall(attributeValues);
    }
}

[DynamoDBMarshaller(typeof(SetDto))]
public partial class DecimalHashSetTests : SetAsserter<HashSet<decimal>, decimal>
{

    public DecimalHashSetTests() : base(new[] {2032m, 0.323232932m, 0.9329392m}, x => new HashSet<decimal>(x))
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(SetDto element)
    {
        return SetDtoMarshaller.Marshall(element);
    }
    protected override SetDto UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return SetDtoMarshaller.Unmarshall(attributeValues);
    }
}