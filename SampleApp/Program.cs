// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        var repository = new PersonDynamoDbRepository();
    }
}


[DynamoDBUpdateOperation(typeof(PersonEntity))]
public partial class PersonDynamoDbRepository 
{
    public void UpdatePerson(PersonEntity entity)
    {
        var request = new UpdateItemRequest()
        {
            Key = UpdatePersonEntityAttributeValueKeys(entity),
            
            
            AttributeUpdates = new Dictionary<string, AttributeValueUpdate>()
            {
                {"", new AttributeValueUpdate(){Action = new AttributeAction(""), Value = new AttributeValue()}}
            }
        };
    }

    private class MyClass
    {
        public static int Foo() => 1;

    }
}
