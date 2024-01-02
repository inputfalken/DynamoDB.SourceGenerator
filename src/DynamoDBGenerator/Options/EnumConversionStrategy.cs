using System;

namespace DynamoDBGenerator.Options;

/// <summary>
///     The strategy to persist and read <see cref="Enum" />.
/// </summary>
public enum EnumConversionStrategy
{
    /// <summary>
    ///     <para>
    ///         Use the associated <see cref="int" /> with
    ///         <see
    ///             cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Marshall" />
    ///         .
    ///     </para>
    ///     <para>
    ///         Use the associated <see cref="int" /> with
    ///         <see
    ///             cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Unmarshall" />
    ///         .
    ///     </para>
    /// </summary>
    Integer = 1,

    /// <summary>
    ///     <para>
    ///         Use the <see cref="Enum.ToString()" /> representation with
    ///         <see
    ///             cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Marshall" />
    ///         .
    ///     </para>
    ///     <para>
    ///         Use case sensitive <see cref="Enum.Parse{TEnum}(System.ReadOnlySpan{char})" /> with
    ///         <see
    ///             cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Unmarshall" />
    ///         .
    ///     </para>
    /// </summary>
    String = 2,

    /// <summary>
    ///     <para>
    ///         Use the <see cref="Enum.ToString()" /> representation with
    ///         <see
    ///             cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Marshall" />
    ///         .
    ///     </para>
    ///     <para>
    ///         Use case insensitive <see cref="Enum.Parse{TEnum}(System.ReadOnlySpan{char})" /> with
    ///         <see
    ///             cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}.Unmarshall" />
    ///         .
    ///     </para>
    /// </summary>
    StringCI = 4
}