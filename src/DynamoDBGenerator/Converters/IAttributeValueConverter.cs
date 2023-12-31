using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters;

/// <summary>
/// Represents a converter for <see cref="AttributeValue"/> where <typeparamref name="T"/> must be a value type.
/// </summary>
/// <typeparam name="T">
/// The type to convert To and From.
/// </typeparam>
public interface IValueTypeConverter<T> where T : struct
{
    /// <summary>
    /// A function responsible for converting <see cref="AttributeValue"/> towards <typeparamref name="T"/>.
    /// </summary>
    /// <param name="attributeValue">
    /// The value map into <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The <typeparamref name="T"/> or <see langword="null" />.
    /// </returns>
    /// <remarks>
    /// You should return <see langword="null" /> if you're unable to perform the mapping.
    /// </remarks>
    public T? Read(AttributeValue attributeValue);

    /// <summary>
    /// A function responsible for converting <typeparamref name="T"/> towards <see cref="AttributeValue"/>
    /// </summary>
    /// <param name="element">
    /// The value to write into an <see cref="AttributeValue"/>.
    /// </param>
    /// <returns>
    /// A mapped <see cref="AttributeValue"/>.
    /// </returns>
    public AttributeValue Write(T element);
}