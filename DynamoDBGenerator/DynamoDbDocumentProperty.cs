using System;

namespace DynamoDBGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBDocumentAttribute : Attribute
{
    private readonly Type _type;
    public DynamoDBDocumentAttribute(Type type)
    {
        _type = type;

    }
}