using System;

namespace DynamoDBGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBDocumentAttribute : Attribute
{
    private readonly Type _entityType;
    public string PropertyName { get; set; }
    public DynamoDBDocumentAttribute(Type entityType)
    {
        _entityType = entityType;
    }
}