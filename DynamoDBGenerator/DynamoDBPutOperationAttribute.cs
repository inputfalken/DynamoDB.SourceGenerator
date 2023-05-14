using System;

namespace DynamoDBGenerator;

// ReSharper disable once InconsistentNaming
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBPutOperationAttribute : Attribute
{
    public Type? Type { get; }

    public DynamoDBPutOperationAttribute(Type type)
    {
        Type = type;
    }
}