using System;
namespace DynamoDBGenerator.Attributes;

/// <summary>
///     When placed on a constructor, indicates that the constructor should be used to create instances of the type when
///     <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Unmarshall" />
///     is used.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class DynamoDBMarshallerConstructorAttribute : Attribute
{
}