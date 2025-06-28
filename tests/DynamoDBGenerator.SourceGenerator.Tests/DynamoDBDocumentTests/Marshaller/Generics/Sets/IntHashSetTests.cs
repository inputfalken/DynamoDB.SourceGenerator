using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<int>>))]
public partial class IntHashSetTests() : SetAsserter<HashSet<int>, int>([2, 3, 4, 5], x => new HashSet<int>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<HashSet<int>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<HashSet<int>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}

// TODO Support
//[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<int?>>))]
//public partial class NUllableIntHashSetTests() : SetAsserter<HashSet<int?>, int?>([2, 3, 4, 5], x => new HashSet<int?>(x))
//{
//    protected override Container<HashSet<int?>> UnmarshallImplementation(
//        Dictionary<string, AttributeValue> attributeValues)
//    {
//        return ContainerMarshaller.Unmarshall(attributeValues);
//    }
//
//    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<HashSet<int?>> element)
//    {
//        return ContainerMarshaller.Marshall(element);
//    }
//}

[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<decimal>>))]
public partial class DecimalHashSetTests : SetAsserter<HashSet<decimal>, decimal>
{
    public DecimalHashSetTests() : base(new[] { 2032m, 0.323232932m, 0.9329392m }, x => new HashSet<decimal>(x))
    {
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<HashSet<decimal>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<HashSet<decimal>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}