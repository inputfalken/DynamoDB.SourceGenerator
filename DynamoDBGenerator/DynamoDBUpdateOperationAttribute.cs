using System;

namespace DynamoDBGenerator;

// ReSharper disable once InconsistentNaming
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBUpdateOperationAttribute : Attribute
{
    public Type? Type { get; }

    public DynamoDBUpdateOperationAttribute(Type type)
    {
        Type = type;
    }
}