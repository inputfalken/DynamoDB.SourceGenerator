using System;

namespace DynamoDBGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBDocument : Attribute
{
    private readonly Type _type;
    public DynamoDBDocument(Type type)
    {
        _type = type;

    }
}