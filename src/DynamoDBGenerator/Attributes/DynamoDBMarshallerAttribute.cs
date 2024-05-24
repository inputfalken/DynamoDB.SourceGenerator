using System;
namespace DynamoDBGenerator.Attributes;

/// <summary>
/// Attribute to generate an implementation of <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}" />
/// for the specified type.
/// </summary>
/// <example>
///     The example below demonstrates the usage of this attribute in a repository class:
///     <code>
///         [DynamoDBMarshaller(EntityType = typeof(OrderEntity), PropertyName = "MyCustomPropertyName"))]
///         public class Repository
///         {
///             public Repository()
///             {
///                 var orderMarshaller = MyCustomPropertyName;
///             }
///         }
///         public class OrderEntity
///         {
///             [DynamoDBHashKey]
///             public string Id { get; set; }
///             public decimal Cost { get; set; }
///         }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class DynamoDBMarshallerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the type that will be used for marshalling and unmarshalling.
    /// </summary>
    public Type? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the name of the property to use when accessing the marshaller.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the type that <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/>
    /// will use as its argument type-parameter.
    /// </summary>
    public Type? ArgumentType { get; set; }

}
