using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

public class OptionalEntityTypeTests : MarshalAsserter<OptionalEnitityTypeDTO>
{
    protected override IEnumerable<(OptionalEnitityTypeDTO element, Dictionary<string, AttributeValue> attributeValues)>
        Arguments()
    {
        var element1 = new OptionalEnitityTypeDTO { SampleField = "Hello" };
        yield return (element1, ToDict(element1));
        var element2 = new OptionalEnitityTypeDTO { SampleField = "Wolrd" };
        yield return (element2, ToDict(element2));

        static Dictionary<string, AttributeValue> ToDict(OptionalEnitityTypeDTO element)
        {
            return new Dictionary<string, AttributeValue>
            {
                { nameof(element.SampleField), new AttributeValue { S = element.SampleField } }
            };
        }
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(OptionalEnitityTypeDTO element)
    {
        return OptionalEnitityTypeDTO.OptionalEnitityTypeDTOMarshaller.Marshall(element);
    }

    protected override OptionalEnitityTypeDTO UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return OptionalEnitityTypeDTO.OptionalEnitityTypeDTOMarshaller.Unmarshall(attributeValues);
    }
}

[DynamoDBMarshaller]
public partial record OptionalEnitityTypeDTO
{
    public string? SampleField { get; set; }
}