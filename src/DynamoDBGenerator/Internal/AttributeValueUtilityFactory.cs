using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Amazon.DynamoDBv2.Model;
using static System.Runtime.InteropServices.CollectionsMarshal;

namespace DynamoDBGenerator.Internal;

/// <summary>
///     This is an internal API that supports the source generator and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new version.
/// </summary>
public static class AttributeValueUtilityFactory
{
#pragma warning disable CS1591
    public static AttributeValue Null { get; } = new() { NULL = true };


    public static AttributeValue FromDictionary<T, TArgument>(
        IEnumerable<KeyValuePair<string, T>> dictionary,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector)
    {
        var elements = dictionary switch
        {
            IReadOnlyDictionary<string, T> a => new Dictionary<string, AttributeValue>(a.Count),
            IDictionary<string, T> a => new Dictionary<string, AttributeValue>(a.Count),
            _ => new Dictionary<string, AttributeValue>()
        };

        foreach (var (key, value) in dictionary)
            elements[key] = resultSelector(value, argument, $"{dataMember}[{key}]");

        return new AttributeValue { M = elements };
    }


    public static ILookup<string, T> ToLookup<T, TArgument>(
        Dictionary<string, AttributeValue> dictionary,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, TArgument, string, T> resultSelector
    )
    {
        return Iterator(dictionary, argument, dataMember, resultSelector)
            .ToLookup(static x => x.Key, static x => x.Value);

        static IEnumerable<KeyValuePair<string, T>> Iterator(
            Dictionary<string, AttributeValue> dictionary,
            TArgument argument,
            string? dataMember,
            Func<AttributeValue, TArgument, string, T> resultSelector
        )
        {
            foreach (var (key, (attributeValue, i)) in dictionary.SelectMany(
                         static x => x.Value.L.Select(static (x, i) => (x, y: i)), static (x, y) => (x.Key, y)))
                yield return new KeyValuePair<string, T>(key,
                    resultSelector(attributeValue, argument, $"{dataMember}[{key}][{i}]"));
        }
    }

    public static AttributeValue FromLookup<T, TArgument>(
        ILookup<string, T> lookup,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector
    )
    {
        var attributeValues = new Dictionary<string, AttributeValue>(lookup.Count);

        foreach (var grouping in lookup)
            attributeValues[grouping.Key] = FromEnumerable(grouping, argument, $"{dataMember}[{grouping.Key}]", resultSelector);

        return new AttributeValue { M = attributeValues };
    }

    public static Dictionary<string, T> ToDictionary<T, TArgument>(
        IReadOnlyDictionary<string, AttributeValue> dictionary,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, string, TArgument, string?, T> resultSelector)
    {
        var elements = new Dictionary<string, T>(dictionary.Count);

        foreach (var (key, value) in dictionary)
            elements[key] = resultSelector(value, key, argument, dataMember);

        return elements;
    }

    public static AttributeValue FromArray<T, TArgument>(
        T[] array,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector)
    {
        var span = array.AsSpan();
        var attributeValues = new List<AttributeValue>(span.Length);
        for (var i = 0; i < span.Length; i++)
            attributeValues.Add(resultSelector(span[i], argument, $"{dataMember}[{i}]"));

        return new AttributeValue { L = attributeValues };
    }

    public static AttributeValue FromList<T, TArgument>(
        List<T> list,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector)
    {
        var span = AsSpan(list);
        var attributeValues = new List<AttributeValue>(span.Length);
        for (var i = 0; i < span.Length; i++)
            attributeValues.Add(resultSelector(span[i], argument, $"{dataMember}[{i}]"));

        return new AttributeValue { L = attributeValues };
    }

    public static AttributeValue FromEnumerable<T, TArgument>(
        IEnumerable<T> enumerable,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector)
    {
        var attributeValues = enumerable.TryGetNonEnumeratedCount(out var count)
            ? new List<AttributeValue>(count)
            : new List<AttributeValue>();

        foreach (var (element, i) in enumerable.Select((x, y) => (x, y)))
            attributeValues.Add(resultSelector(element, argument, $"{dataMember}[{i}]"));

        return new AttributeValue { L = attributeValues };
    }

    public static List<TResult> ToList<TResult, TArgument>(
        List<AttributeValue> attributeValues,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, int, TArgument, string?, TResult> resultSelector
    )
    {
        var span = AsSpan(attributeValues);
        var elements = new List<TResult>(span.Length);
        for (var i = 0; i < span.Length; i++)
            elements.Add(resultSelector(span[i], i, argument, dataMember));

        return elements;
    }

    public static IEnumerable<TResult> ToEnumerable<TResult, TArgument>(
        List<AttributeValue> attributeValues,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, int, TArgument, string?, TResult> resultSelector
    )
    {
        for (var i = 0; i < attributeValues.Count; i++)
            yield return resultSelector(attributeValues[i], i, argument, dataMember);
    }

    public static TResult[] ToArray<TResult, TArgument>(
        List<AttributeValue> attributeValues,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, int, TArgument, string?, TResult> resultSelector
    )
    {
        var span = AsSpan(attributeValues);
        var elements = new TResult[span.Length];
        for (var i = 0; i < span.Length; i++)
            elements[i] = resultSelector(span[i], i, argument, dataMember);

        return elements;
    }
#pragma warning restore CS1591
}