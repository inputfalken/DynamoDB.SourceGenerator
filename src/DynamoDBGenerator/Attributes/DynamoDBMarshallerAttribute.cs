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
    /// <remarks>
    /// The default value will be the type where the attribute was applied to.
    /// </remarks>
    public Type? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the name of the property to use when accessing the marshaller.
    /// </summary>
    /// <remarks>
    /// The default value will be dependant on the <see cref="EntityType"/> by having the naming format of `{Type.Name}Marshaller`.
    /// </remarks>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the type that <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/>
    /// will use as its argument type-parameter.
    /// </summary>
    /// <remarks>
    /// The default value will be <see cref="EntityType"/>, this is will make the the generated code be implemented in a PUT oriented manner.
    /// </remarks>
    public Type? ArgumentType { get; set; }

}
