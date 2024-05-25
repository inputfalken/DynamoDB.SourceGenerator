using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Lookups;

[DynamoDBMarshaller(EntityType = typeof(Container<ILookup<string, Entity>>))]
public partial class ILookupTests : RecordMarshalAsserter<ILookup<string, ILookupTests.Entity>>
{
    public record Entity(string Foo, string Bar);

    public ILookupTests() : base(new[]
    {
        new List<string>
        {
            "abc", "dfg"
        }.ToLookup(x => x.Length.ToString(), s => new Entity(s, s)),
        new List<string>
        {
            "hello", "world", "aa"
        }.ToLookup(x => x.Length.ToString(), s => new Entity(s, s)),
        new List<string>
        {
            "", "world"
        }.ToLookup(x => x.Length.ToString(), s => new Entity(s, s))
    }, x =>
    {
        return new AttributeValue
        {
            M = x.ToDictionary(y => y.Key, y => new AttributeValue
                {
                    L = y.Select(z => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {
                                nameof(z.Bar), new AttributeValue
                                {
                                    S = z.Bar
                                }
                            },
                            {
                                nameof(z.Foo), new AttributeValue
                                {
                                    S = z.Foo
                                }
                            }
                        }
                    }).ToList()
                }
            )
        };
    })
    {
    }

    protected override Container<ILookup<string, Entity>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(
        Container<ILookup<string, Entity>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

//[DynamoDBMarshaller(EntityType = typeof(Container<ILookup<string, Entity?>>))]
//public partial class
//    NullableILookupElementsTests : RecordMarshalAsserter<ILookup<string, NullableILookupElementsTests.Entity?>>
//{
//    public record Entity(string Foo, string Bar);
//
//    public NullableILookupElementsTests() : base(new ILookup<string, Entity?>[]
//    {
//        new List<string>
//        {
//            "abc", "dfg"
//        }.ToLookup(x => x.Length.ToString(), s => new Entity(s, s))!,
//        new List<string>
//        {
//            "hello", "world", "aa"
//        }.ToLookup(x => x.Length.ToString(), s => new Entity(s, s))!,
//        new List<string>
//        {
//            "", "world"
//        }.ToLookup(x => x.Length.ToString(), s => new Entity(s, s))!,
//        new List<string>
//        {
//            "", "world"
//        }.ToLookup(x => x.Length.ToString(), s => (Entity)null!)!,
//    }, x =>
//    {
//        return new AttributeValue
//        {
//            M = x.Where(x => x.Any(y => y is not null)).ToDictionary(y => y.Key, y => new AttributeValue
//                {
//                    L = y.Select(z => new AttributeValue
//                    {
//                        M = new Dictionary<string, AttributeValue>
//                        {
//                            {
//                                nameof(z.Bar), new AttributeValue
//                                {
//                                    S = z!.Bar
//                                }
//                            },
//                            {
//                                nameof(z.Foo), new AttributeValue
//                                {
//                                    S = z.Foo
//                                }
//                            }
//                        }
//                    }).ToList()
//                }
//            )
//        };
//    })
//    {
//    }
//
//    protected override Container<ILookup<string, Entity?>> UnmarshallImplementation(
//        Dictionary<string, AttributeValue> attributeValues)
//    {
//        return ContainerMarshaller.Unmarshall(attributeValues);
//    }
//
//    protected override Dictionary<string, AttributeValue> MarshallImplementation(
//        Container<ILookup<string, Entity?>> element)
//    {
//        return ContainerMarshaller.Marshall(element);
//    }
//}
