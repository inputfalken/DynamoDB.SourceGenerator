using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Internal;

/// <summary>
///     This is an internal API that supports the source generator and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new version.
/// </summary>
public static class MarshallerFactory
{
#pragma warning disable CS1591
    public static AttributeValue Null { get; } = new() { NULL = true };

    public static List<T> UnmarshallList<T>(List<AttributeValue> source, Func<AttributeValue, int, T> elementMapper)
    {
        var span = CollectionsMarshal.AsSpan(source);
        var list = new List<T>(span.Length);

        for (var i = 0; i < span.Length; i++)
            list[i] = elementMapper(span[i], i);

        return list;
    }
    
    public static T[] UnmarshallArray<T>(List<AttributeValue> source, Func<AttributeValue, int, T> elementMapper)
    {
        var span = CollectionsMarshal.AsSpan(source);
        var array = new T[span.Length];

        for (var i = 0; i < span.Length; i++)
            array[i] = elementMapper(span[i], i);

        return array;
    }


    public static IEnumerable<T> IEnumerable<T>(AttributeValue source, Func<AttributeValue, T> elementMapper)
    {
        return source.L.Select(elementMapper);
    }


    public static AttributeValue List<T>(List<T> source, Func<T, AttributeValue> elementMapper)
    {
        var span = CollectionsMarshal.AsSpan(source);
        var attributeValues = new List<AttributeValue>(source.Count);
        for (var i = 0; i < span.Length; i++)
            attributeValues[i] = elementMapper(span[i]);

        return new AttributeValue { L = attributeValues };
    }

    public static AttributeValue IEnumerable<T>(IEnumerable<T> source, Func<T, AttributeValue> mapper)
    {
        return new AttributeValue { L = new List<AttributeValue>(source.Select(mapper)) };
    }

    public static AttributeValue ICollection<T>(ICollection<T> source, Func<T, AttributeValue> mapper)
    {
        var list = new List<AttributeValue>(source.Count);
        list.AddRange(source.Select(mapper));

        return new AttributeValue { L = list };
    }

    public static AttributeValue IReadOnlyCollection<T>(IReadOnlyCollection<T> source, Func<T, AttributeValue> mapper)
    {
        var list = new List<AttributeValue>(source.Count);
        list.AddRange(source.Select(mapper));

        return new AttributeValue { L = list };
    }

    public static AttributeValue IReadOnlyList<T>(IReadOnlyList<T> source, Func<T, AttributeValue> mapper)
    {
        var attributeValues = new List<AttributeValue>(source.Count);
        for (var i = 0; i < source.Count; i++)
            attributeValues[i] = mapper(source[i]);

        return new AttributeValue { L = attributeValues };
    }

    public static AttributeValue IList<T>(IList<T> source, Func<T, AttributeValue> mapper)
    {
        var attributeValues = new List<AttributeValue>(source.Count);
        for (var i = 0; i < source.Count; i++)
            attributeValues[i] = mapper(source[i]);

        return new AttributeValue { L = attributeValues };
    }

    public static AttributeValue Array<T>(T[] source, Func<T, AttributeValue> mapper)
    {
        var span = source.AsSpan();
        var attributeValues = new List<AttributeValue>(source.Length);
        for (var i = 0; i < span.Length; i++)
            attributeValues[i] = mapper(span[i]);

        return new AttributeValue { L = attributeValues };
    }
#pragma warning restore CS1591
}