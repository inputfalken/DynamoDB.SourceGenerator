using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Dictionaries;

[DynamoDBMarshaller(typeof(Container<Dictionary<string, Entity>>))]
public partial class DictionaryTests : RecordMarshalAsserter<Dictionary<string, DictionaryTests.Entity>>
{
    private const string Element1 = "hello";
    private const string Element2 = "world";

    public DictionaryTests() : base(new[]
    {
        new Dictionary<string, Entity>
        {
            { Element1,new Entity("hey", "ho") },
            { Element2, new ("hey1", "ho1") }
        },
        new Dictionary<string, Entity>
        {
            { Element2, new Entity("something", "else") },
            { Element1, new Entity("something1", "else1") }
        }
    }, x => new AttributeValue
    {
        M = x.ToDictionary(y => y.Key, y => new AttributeValue
            {
                M = new Dictionary<string, AttributeValue>
                {
                    { nameof(y.Value.Foo), new AttributeValue { S = y.Value.Foo } },
                    { nameof(y.Value.Bar), new AttributeValue { S = y.Value.Bar } }
                }
            }
        )
    })
    {
    }


    protected override Container<Dictionary<string, Entity>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(
        Container<Dictionary<string, Entity>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public record Entity(string Foo, string Bar);
}

//[DynamoDBMarshaller(typeof(Container<Dictionary<string, Entity?>>))]
//public partial class
//    NullableDictionaryElementsTests : RecordMarshalAsserter<Dictionary<string, NullableDictionaryElementsTests.Entity?>>
//{
//    private const string Element1 = "hello";
//    private const string Element2 = "world";
//
//    public NullableDictionaryElementsTests() : base(new Dictionary<string, Entity?>[]
//    {
//        new()
//        {
//            { Element1, new Entity("hey", "ho") },
//            { Element2, new Entity("hey1", "ho1") }
//        },
//        new()
//        {
//            { Element2, new Entity("something", "else") },
//            { Element1, new Entity("something1", "else1") }
//        },
//        new()
//        {
//            { "I dont exist", null },
//        }
//    }, x =>
//    {
//        return new AttributeValue
//        {
//            M = x
//                .Where(y => y.Value is not null)
//                .ToDictionary(y => y.Key, y => new AttributeValue
//                    {
//                        M = new Dictionary<string, AttributeValue>
//                        {
//                            { nameof(y.Value.Foo), new AttributeValue { S = y.Value!.Foo } },
//                            { nameof(y.Value.Bar), new AttributeValue { S = y.Value.Bar } }
//                        }
//                    }
//                )
//        };
//    })
//    {
//    }
//
//
//    protected override Container<Dictionary<string, Entity?>> UnmarshallImplementation(
//        Dictionary<string, AttributeValue> attributeValues)
//    {
//        return ContainerMarshaller.Unmarshall(attributeValues);
//    }
//
//    protected override Dictionary<string, AttributeValue> MarshallImplementation(
//        Container<Dictionary<string, Entity?>> element)
//    {
//        return ContainerMarshaller.Marshall(element);
//    }
//    public record Entity(string Foo, string Bar);
//}