using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.Namespace.A;

public class NamespaceATests
{
    [Fact]
    public void Verify_Correct_NameType()
    {
        Container.ContainerMarshaller
            .AttributeExpressionNameTracker()
            .Should()
            .BeOfType<Container.ContainerNames>();

        Container.ContainerMarshaller
            .AttributeExpressionNameTracker()
            .PropertyA
            .Should()
            .BeOfType<Container.SampleClassNames>();
    }

    [Fact]
    public void Verify_Correct_ValueType()
    {
        Container.ContainerMarshaller
            .AttributeExpressionValueTracker()
            .Should()
            .BeOfType<Container.ContainerValues>();

        Container.ContainerMarshaller
            .AttributeExpressionValueTracker()
            .PropertyA
            .Should()
            .BeOfType<Container.SampleClassValues>();
    }

    [Fact]
    public void Unmarshall()
    {
        Container.ContainerMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(Container.PropertyA),
                    new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                            { { nameof(Container.PropertyA.Property), new AttributeValue { N = "2" } } }
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
                        nameof(Container.PropertyA),
                        new AttributeValue
                        {
                            M = new Dictionary<string, AttributeValue>
                                { { nameof(Container.PropertyA.Property), new AttributeValue { N = "2" } } }
                        }
                    }
                }
            );
    }
}

[DynamoDBMarshaller]
public partial record Container(SampleClass PropertyA);