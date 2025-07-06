using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Internal;

/// <summary>
///     This is an internal API that supports the source generator and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new version.
/// </summary>
public static class MarshallHelper
{
#pragma warning disable CS1591
    public static AttributeValue Null { get; } = new() { NULL = true };

    [return: NotNullIfNotNull(nameof(dict))]
    public static AttributeValue? ToAttributeValue(
        [NotNullIfNotNull(nameof(dict))] Dictionary<string, AttributeValue>? dict
    )
    {
        return dict is null
            ? null
            : new AttributeValue { M = dict };
    }

    public static AttributeValue FromDictionary<T, TArgument>(
        IEnumerable<KeyValuePair<string, T>> dictionary,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector)
    {
        Dictionary<string, AttributeValue> dict;
        if (dictionary.TryGetNonEnumeratedCount(out var count))
        {
            if (count is 0)
                return new AttributeValue { M = [] };

            dict = new Dictionary<string, AttributeValue>(count);
        }
        else
            dict = [];

        foreach (var (key, value) in dictionary)
            dict[key] = resultSelector(value, argument, $"{dataMember}[{key}]");

        return new AttributeValue { M = dict };
    }

    public static AttributeValue FromNullableNumberSet<T>(IEnumerable<T?> numbers, string? _)
        where T : struct, INumber<T>
    {
        if (numbers.TryGetNonEnumeratedCount(out var count) is false)
            return new AttributeValue
            {
                NS = numbers.Select(number => number?.ToString()).ToList()
            };

        if (count is 0)
            return new AttributeValue { NS = [] };

        var list = new List<string?>(count);
        list.AddRange(numbers.Select(number => number?.ToString()));

        return new AttributeValue { NS = list };
    }

    public static AttributeValue FromNumberSet<T>(IEnumerable<T> numbers, string? dataMember)
        where T : struct, INumber<T>
    {
        if (numbers.TryGetNonEnumeratedCount(out var count) is false)
        {
            var noCapacity = new List<string>();

            foreach (var number in numbers)
            {
                var @string = number.ToString();
                if (string.IsNullOrEmpty(@string))
                    throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");

                noCapacity.Add(@string);
            }

            return new AttributeValue { NS = noCapacity };
        }

        if (count is 0)
            return new AttributeValue { NS = [] };

        var list = new List<string>(count);

        foreach (var number in numbers)
        {
            var @string = number.ToString();
            if (string.IsNullOrEmpty(@string))
                throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");

            list.Add(@string);
        }

        return new AttributeValue { NS = list };
    }

    public static AttributeValue FromNullableStringSet(IEnumerable<string?> strings, string? _)
    {
        if (strings.TryGetNonEnumeratedCount(out var count) is false)
            return new AttributeValue { SS = strings.ToList() };

        if (count is 0)
            return new AttributeValue { SS = [] };

        var list = new List<string?>(count);
        list.AddRange(strings);

        return new AttributeValue { SS = list };
    }

    public static AttributeValue FromStringSet(IEnumerable<string> strings, string? dataMember)
    {
        if (strings.TryGetNonEnumeratedCount(out var count) is false)
        {
            var list = new List<string>();
            foreach (var @string in strings)
            {
                if (@string is null)
                    throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");
                list.Add(@string);
            }

            return new AttributeValue { SS = list };
        }
        else
        {
            if (count is 0)
                return new AttributeValue { SS = [] };

            var list = new List<string>(count);

            foreach (var @string in strings)
                list.Add(@string ?? throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]"));

            return new AttributeValue { SS = list };
        }
    }

    private static TSet ToStringSet<TSet>(
        List<string> numbers,
        Func<int, TSet> factory,
        string? dataMember
    )
        where TSet : ICollection<string>
    {
        var span = CollectionsMarshal.AsSpan(numbers);
        var set = factory(span.Length);

        foreach (var @string in span)
        {
            if (@string is null)
                throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");

            set.Add(@string);
        }

        return set;
    }

    private static TSet ToNullableStringSet<TSet>(
        List<string?> numbers,
        Func<int, TSet> factory,
        string? _
    )
        where TSet : ICollection<string?>
    {
        var span = CollectionsMarshal.AsSpan(numbers);
        var set = factory(span.Length);

        foreach (var @string in span)
            set.Add(@string);

        return set;
    }

    private static TSet ToNumberSet<TNumber, TSet>(
        List<string> numbers,
        Func<int, TSet> factory,
        string? dataMember
    )
        where TSet : ICollection<TNumber>
        where TNumber : struct, INumber<TNumber>
    {
        var span = CollectionsMarshal.AsSpan(numbers);
        var set = factory(span.Length);

        foreach (var number in span)
        {
            if (number is null)
                throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");

            set.Add(TNumber.Parse(number, null));
        }

        return set;
    }

    private static TSet ToNullableNumberSet<TNumber, TSet>(
        List<string?> numbers,
        Func<int, TSet> factory,
        string? _
    )
        where TSet : ICollection<TNumber?>
        where TNumber : struct, INumber<TNumber>
    {
        var span = CollectionsMarshal.AsSpan(numbers);
        var set = factory(span.Length);

        foreach (var number in span)
        {
            if (number is null)
                set.Add(null);
            else
                set.Add(TNumber.Parse(number, null));
        }

        return set;
    }

    public static ISet<string> ToStringISet(List<string> ss, string? dataMember)
    {
        return ToStringSet<HashSet<string>>(ss, i => new HashSet<string>(i), dataMember);
    }

    public static IReadOnlySet<string> ToStringIReadOnlySet(List<string> ss, string? dataMember)
    {
        return ToStringSet<HashSet<string>>(ss, i => new HashSet<string>(i), dataMember);
    }

    public static HashSet<string> ToStringHashSet(List<string> ss, string? dataMember)
    {
        return ToStringSet<HashSet<string>>(ss, i => new HashSet<string>(i), dataMember);
    }

    public static ISet<string?> ToNullableStringISet(List<string?> ss, string? dataMember)
    {
        return ToNullableStringSet<HashSet<string?>>(ss, i => new HashSet<string?>(i), dataMember);
    }

    public static IReadOnlySet<string?> ToNullableStringIReadOnlySet(List<string?> ss, string? dataMember)
    {
        return ToNullableStringSet<HashSet<string?>>(ss, i => new HashSet<string?>(i), dataMember);
    }

    public static HashSet<string?> ToNullableStringHashSet(List<string?> ss, string? dataMember)
    {
        return ToNullableStringSet<HashSet<string?>>(ss, i => new HashSet<string?>(i), dataMember);
    }

    public static SortedSet<string?> ToNullableStringSortedSet(List<string?> ss, string? _)
    {
        var span = CollectionsMarshal.AsSpan(ss);
        var set = new SortedSet<string?>();
        foreach (var se in span)
            set.Add(se);

        return set;
    }

    public static SortedSet<string> ToStringSortedSet(List<string> ss, string? dataMember)
    {
        var span = CollectionsMarshal.AsSpan(ss);
        var set = new SortedSet<string>();
        foreach (var se in span)
        {
            if (se is null)
                throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");

            set.Add(se);
        }

        return set;
    }

    public static ISet<TNumber> ToNumberISet<TNumber>(List<string> ns, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        return ToNumberSet<TNumber, HashSet<TNumber>>(ns, i => new HashSet<TNumber>(i), dataMember);
    }

    public static IReadOnlySet<TNumber> ToNumberIReadOnlySet<TNumber>(List<string> ns, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        return ToNumberSet<TNumber, HashSet<TNumber>>(ns, i => new HashSet<TNumber>(i), dataMember);
    }

    public static HashSet<TNumber> ToNumberHashSet<TNumber>(List<string> ss, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        return ToNumberSet<TNumber, HashSet<TNumber>>(ss, i => new HashSet<TNumber>(i), dataMember);
    }

    public static SortedSet<TNumber> ToNumberSortedSet<TNumber>(List<string> ns, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        var span = CollectionsMarshal.AsSpan(ns);
        var set = new SortedSet<TNumber>();
        foreach (var se in span)
        {
            if (se is null)
                throw ExceptionHelper.NotNull($"{dataMember}[UNKNOWN]");

            set.Add(TNumber.Parse(se, null));
        }

        return set;
    }

    public static ISet<TNumber?> ToNullableNumberISet<TNumber>(List<string?> ns, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        return ToNullableNumberSet<TNumber, HashSet<TNumber?>>(ns, i => new HashSet<TNumber?>(i), dataMember);
    }

    public static IReadOnlySet<TNumber?> ToNullableNumberIReadOnlySet<TNumber>(List<string?> ns, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        return ToNullableNumberSet<TNumber, HashSet<TNumber?>>(ns, i => new HashSet<TNumber?>(i), dataMember);
    }

    public static HashSet<TNumber?> ToNullableNumberHashSet<TNumber>(List<string?> ns, string? dataMember)
        where TNumber : struct, INumber<TNumber>
    {
        return ToNullableNumberSet<TNumber, HashSet<TNumber?>>(ns, i => new HashSet<TNumber?>(i), dataMember);
    }

    public static SortedSet<TNumber?> ToNullableNumberSortedSet<TNumber>(List<string?> ns, string? _)
        where TNumber : struct, INumber<TNumber>
    {
        return new SortedSet<TNumber?>(ns.Select(x => x is null ? (TNumber?)null : TNumber.Parse(x, null)));
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
            attributeValues[grouping.Key] =
                FromEnumerable(grouping, argument, $"{dataMember}[{grouping.Key}]", resultSelector);

        return new AttributeValue { M = attributeValues };
    }

    public static Dictionary<string, T> ToDictionary<T, TArgument>(
        IReadOnlyDictionary<string, AttributeValue> dictionary,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, TArgument, string?, T> resultSelector)
    {
        if (dictionary.Count is 0)
            return [];

        var elements = new Dictionary<string, T>(dictionary.Count);

        foreach (var (key, value) in dictionary)
            elements[key] = resultSelector(value, argument, $"{dataMember}[{key}]");

        return elements;
    }

    public static AttributeValue FromArray<T, TArgument>(
        T[] array,
        TArgument argument,
        string? dataMember,
        Func<T, TArgument, string?, AttributeValue> resultSelector)
    {
        var span = array.AsSpan();
        if (span.Length is 0)
            return new AttributeValue { L = [] };
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
        var span = CollectionsMarshal.AsSpan(list);
        if (span.Length is 0)
            return new AttributeValue { L = [] };

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
        if (enumerable.TryGetNonEnumeratedCount(out var count) is false)
            return new AttributeValue
            {
                L = [..enumerable.Select((element, i) => resultSelector(element, argument, $"{dataMember}[{i}]"))]
            };

        if (count == 0)
            return new AttributeValue { L = [] };

        var list = new List<AttributeValue>(count);
        foreach (var (element, i) in enumerable.Select((x, y) => (x, y)))
            list.Add(resultSelector(element, argument, $"{dataMember}[{i}]"));

        return new AttributeValue { L = list };
    }

    public static List<TResult> ToList<TResult, TArgument>(
        List<AttributeValue> attributeValues,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, TArgument, string?, TResult> resultSelector
    )
    {
        var span = CollectionsMarshal.AsSpan(attributeValues);
        if (span.Length is 0)
            return [];

        var elements = new List<TResult>(span.Length);
        for (var i = 0; i < span.Length; i++)
            elements.Add(resultSelector(span[i], argument, $"{dataMember}[{i}]"));

        return elements;
    }

    public static IEnumerable<TResult> ToEnumerable<TResult, TArgument>(
        List<AttributeValue> attributeValues,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, TArgument, string?, TResult> resultSelector
    )
    {
        return attributeValues.Count is 0
            ? []
            : Iterator(attributeValues, argument, dataMember, resultSelector);

        static IEnumerable<TResult> Iterator(
            List<AttributeValue> list,
            TArgument argument,
            string? dataMember,
            Func<AttributeValue, TArgument, string?, TResult> resultSelector
        )
        {
            for (var i = 0; i < list.Count; i++)
                yield return resultSelector(list[i], argument, $"{dataMember}[{i}]");
        }
    }

    public static TResult[] ToArray<TResult, TArgument>(
        List<AttributeValue> attributeValues,
        TArgument argument,
        string? dataMember,
        Func<AttributeValue, TArgument, string?, TResult> resultSelector
    )
    {
        var span = CollectionsMarshal.AsSpan(attributeValues);
        if (span.Length is 0)
            return [];
        
        var elements = new TResult[span.Length];
        for (var i = 0; i < span.Length; i++)
            elements[i] = resultSelector(span[i], argument, $"{dataMember}[{i}]");

        return elements;
    }
#pragma warning restore CS1591
}