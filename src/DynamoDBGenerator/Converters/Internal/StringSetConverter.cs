using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class StringSetConverter :
    IReferenceTypeConverter<IReadOnlySet<string>>,
    IReferenceTypeConverter<HashSet<string>>,
    IReferenceTypeConverter<ISet<string>>,
    IReferenceTypeConverter<SortedSet<string>>,
    IStaticSingleton<StringSetConverter>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static HashSet<string>? ToHashSet(AttributeValue attributeValue)
    {
        return attributeValue.IsSSSet
            ? new HashSet<string>(attributeValue.SS)
            : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AttributeValue ToAttributeValue(IEnumerable<string> enumerable)
    {
        return new AttributeValue { SS = new List<string>(enumerable) };
    }

    IReadOnlySet<string>? IReferenceTypeConverter<IReadOnlySet<string>>.Read(AttributeValue attributeValue)
    {
        return ToHashSet(attributeValue);
    }

    ISet<string>? IReferenceTypeConverter<ISet<string>>.Read(AttributeValue attributeValue)
    {
        return ToHashSet(attributeValue);
    }

    HashSet<string>? IReferenceTypeConverter<HashSet<string>>.Read(AttributeValue attributeValue)
    {
        return ToHashSet(attributeValue);
    }

    SortedSet<string>? IReferenceTypeConverter<SortedSet<string>>.Read(AttributeValue attributeValue)
    {
        return attributeValue.IsSSSet
            ? new SortedSet<string>(attributeValue.SS)
            : null;
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