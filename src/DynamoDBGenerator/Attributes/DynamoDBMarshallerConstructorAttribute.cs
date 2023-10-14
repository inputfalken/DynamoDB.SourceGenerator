using System;
namespace DynamoDBGenerator.Attributes;

/// <summary>
/// Attribute used on a constructor to indicate that it should be used for creating instances of the type during the 
/// <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Unmarshall" />
/// process.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class DynamoDBMarshallerConstructorAttribute : Attribute
{
}