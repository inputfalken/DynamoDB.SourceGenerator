using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class NoneNullableStringSetConverter :
    IReferenceTypeConverter<IReadOnlySet<string>>,
    IReferenceTypeConverter<HashSet<string>>,
    IReferenceTypeConverter<ISet<string>>,
    IReferenceTypeConverter<SortedSet<string>>,
    IStaticSingleton<NoneNullableStringSetConverter>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSet? ToSet<TSet>(
        AttributeValue attributeValue,
        Func<int, TSet> factory
    )
        where TSet : class, ICollection<string>
    {
        if (attributeValue.IsSSSet is false)
            return null;

        var span = CollectionsMarshal.AsSpan(attributeValue.SS);
        var set = factory(span.Length);
        foreach (var item in span)
        {
            if (item is null) // TODO need datamember
                throw ExceptionHelper.NotNull(null);

            set.Add(item);
        }

        return set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AttributeValue ToAttributeValue(IEnumerable<string> enumerable)
    {
        var list = enumerable.TryGetNonEnumeratedCount(out var count)
            ? new List<string>(capacity: count)
            : [];

        foreach (var item in enumerable)
        {
            if (item is null) // TODO need datamember
                throw ExceptionHelper.NotNull(null);

            list.Add(item);
        }

        return new AttributeValue { SS = list };
    }

    IReadOnlySet<string>? IReferenceTypeConverter<IReadOnlySet<string>>.Read(AttributeValue attributeValue)
    {
        return ToSet(attributeValue, x => new HashSet<string>(x));
    }

    ISet<string>? IReferenceTypeConverter<ISet<string>>.Read(AttributeValue attributeValue)
    {
        return ToSet(attributeValue, x => new HashSet<string>(x));
    }

    HashSet<string>? IReferenceTypeConverter<HashSet<string>>.Read(AttributeValue attributeValue)
    {
        return ToSet(attributeValue, x => new HashSet<string>(x));
    }

    SortedSet<string>? IReferenceTypeConverter<SortedSet<string>>.Read(AttributeValue attributeValue)
    {
        return ToSet(attributeValue, _ => new SortedSet<string>());
    }

    AttributeValue IReferenceTypeConverter<SortedSet<string>>.Write(SortedSet<string> element)
    {
        return ToAttributeValue(element);
    }

    AttributeValue IReferenceTypeConverter<ISet<string>>.Write(ISet<string> element)
    {
        return ToAttributeValue(element);
    }

    AttributeValue IReferenceTypeConverter<HashSet<string>>.Write(HashSet<string> element)
    {
        return ToAttributeValue(element);
    }

    AttributeValue IReferenceTypeConverter<IReadOnlySet<string>>.Write(IReadOnlySet<string> element)
    {
        return ToAttributeValue(element);
    }
}