using System;
namespace DynamoDBGenerator.Attributes;

/// <summary>
/// Attribute used to source generate an implementation of <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}" />.
/// </summary>
/// <example>
///     The example below demonstrates a console app where this attribute is used:
///     <code>
///         public class Program
///         {
///             public static void Main(string[] args)
///             {
///                 var orderEntity = new OrderEntity 
///                                    {
///                                        Id = "1",
///                                        Cost = 2.3
///                                    };
///                 var attributeValues  = OrderEntity.MyCustomPropertyName.Marshall(orderEntity);
///                 foreach(var keyValue in attributeValues)
///                 {
///                     Console.WriteLine(attributeValues);
///                 }
///             }
///         }
///         [DynamoDBMarshaller(EntityType = typeof(OrderEntity), PropertyName = "MyCustomPropertyName"))]
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
    /// The default value will be dependant on the <see cref="EntityType"/> by having the naming format of `{Type.Name}Marshaller` but without the reflection.
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
