using System;

namespace DynamoDBGenerator;

// ReSharper disable once InconsistentNaming
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDbDocument : Attribute
{
    public Type? Type { get; }

    public DynamoDbDocument(Type type)
    {
        Type = type;
    }
}