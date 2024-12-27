using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.Namespace.B;

public class NamespaceBTests
{
    
    [Fact]
    public void Verify_Correct_NameType()
    {
        Container.ContainerMarshaller
            .AttributeExpressionNameTracker()
            .Should()
            .BeOfType<B.Container.ContainerNames>();
        
        Container.ContainerMarshaller
            .AttributeExpressionNameTracker()
            .PropertyB
            .Should()
            .BeOfType<B.Container.SampleClassNames>();
    }

    [Fact]
    public void Verify_Correct_ValueType()
    {
        Container.ContainerMarshaller
            .AttributeExpressionValueTracker()
            .Should()
            .BeOfType<B.Container.ContainerValues>();
        
        Container.ContainerMarshaller
            .AttributeExpressionValueTracker()
            .PropertyB
            .Should()
            .BeOfType<B.Container.SampleClassValues>();
    }

    [Fact]
    public void Unmarshall()
    {
        Container.ContainerMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(Container.PropertyB),
                    new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                            { { nameof(Container.PropertyB.Property), new AttributeValue { N = "2" } } }
                    }
                }
            })
            .Should()
            .BeEquivalentTo(new Container(new SampleClass(2)));
    }

    [Fact]
    public void Marshall()
    {
        Container.ContainerMarshaller
            .Marshall(new Container(new SampleClass(2)))
            .Should()
            .BeEquivalentTo(new Dictionary<string, AttributeValue>
                {
                    {
                        nameof(Container.PropertyB),
                        new AttributeValue
                        {
                            M = new Dictionary<string, AttributeValue>
                                { { nameof(Container.PropertyB.Property), new AttributeValue { N = "2" } } }
                        }
                    }
                }
            );
    }
}

[DynamoDBMarshaller]
public partial record Container(SampleClass PropertyB);