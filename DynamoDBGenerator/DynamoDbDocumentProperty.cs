﻿using System;

namespace DynamoDBGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBMarshallertAttribute : Attribute
{
    private readonly Type _entityType;

    /// <summary>
    /// Specifies the name of the property to use when accessing the source generated functionality.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public Type? ArgumentType { get; set; }
    public DynamoDBMarshallertAttribute(Type entityType)
    {
        _entityType = entityType;
    }
}