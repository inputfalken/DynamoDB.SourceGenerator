using System;
namespace DynamoDBGenerator.Attributes;

/// <summary>
///     When placed on a class it will generate an implementation of
///     <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}" /> for the
///     specified type.
///     The example below shows an example of this attribute being used in a repository.
/// </summary>
/// <example>
///     <code>
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
    ///     Constructs an <see cref="DynamoDBMarshallerAttribute" /> for source generation purposes.
    /// </summary>
    /// <param name="entityType">
    ///     The type to be represented as an DynamoDB entity.
    /// </param>
    public DynamoDBMarshallerAttribute(Type entityType)
    {
        _entityType = entityType;
    }

    /// <summary>
    ///     Specifies the name of the property to use when accessing
    ///     <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}" />.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    ///     Will set the type
    ///     <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}" /> will
    ///     use as its argument type-parameter.
    /// </summary>
    public Type? ArgumentType { get; set; }
}