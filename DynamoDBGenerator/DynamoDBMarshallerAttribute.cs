using System;

namespace DynamoDBGenerator;

/// <summary>
/// Used to generate an <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/>  implementation.
/// The example below shows an example of this attribute being used in a repository.
/// </summary>
/// <example>
/// <code>
/// [DynamoDBMarshaller(typeof(OrderEntity), PropertyName = "MyCustomPropertyName")]
/// public class Repository
/// {
///     public Repository()
///     {
///         var orderMarshaller = MyCustomPropertyName;
///     }
/// }
/// public class OrderEntity
/// {
///     [DynamoDBHashKey]
///     public string Id { get; set; }
///     public decimal Cost { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBMarshallerAttribute : Attribute
{
    // ReSharper disable once NotAccessedField.Local
    private readonly Type _entityType;

    /// <summary>
    /// Specifies the name of the property to use when accessing <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/>.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Will set the type <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/> will use as its argument type-parameter.
    /// </summary>
    public Type? ArgumentType { get; set; }

    /// <summary>
    ///  Constructs an <see cref="DynamoDBMarshallerAttribute"/> for source generation purposes.
    /// </summary>
    /// <param name="entityType">
    /// The type to be represented as an DynamoDB entity.
    /// </param>
    public DynamoDBMarshallerAttribute(Type entityType)
    {
        _entityType = entityType;
    }
}

[AttributeUsage(AttributeTargets.Constructor)]
public class DynamoDBMarshallerConstructor : Attribute
{

}