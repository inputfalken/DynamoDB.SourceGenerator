using System;
namespace DynamoDBGenerator;

[AttributeUsage(AttributeTargets.Class)]
public class DynamoDbDocument : Attribute
{

    public DynamoDbDocument()
    {
    }
}