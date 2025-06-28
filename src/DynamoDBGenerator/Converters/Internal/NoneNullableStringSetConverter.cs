using System.Collections.Generic;
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

    IReadOnlySet<string>? IReferenceTypeConverter<IReadOnlySet<string>>.Read(AttributeValue attributeValue)
    {
        return MarshallHelper.ToStringSet(attributeValue, x => new HashSet<string>(x));
    }

    ISet<string>? IReferenceTypeConverter<ISet<string>>.Read(AttributeValue attributeValue)
    {
        return MarshallHelper.ToStringSet(attributeValue, x => new HashSet<string>(x));
    }

    HashSet<string>? IReferenceTypeConverter<HashSet<string>>.Read(AttributeValue attributeValue)
    {
        return MarshallHelper.ToStringSet(attributeValue, x => new HashSet<string>(x));
    }

    SortedSet<string>? IReferenceTypeConverter<SortedSet<string>>.Read(AttributeValue attributeValue)
    {
        return MarshallHelper.ToStringSet(attributeValue, _ => new SortedSet<string>());
    }

    AttributeValue IReferenceTypeConverter<SortedSet<string>>.Write(SortedSet<string> element)
    {
        return MarshallHelper.FromStringSet(element);
    }

    AttributeValue IReferenceTypeConverter<ISet<string>>.Write(ISet<string> element)
    {
        return MarshallHelper.FromStringSet(element);
    }

    AttributeValue IReferenceTypeConverter<HashSet<string>>.Write(HashSet<string> element)
    {
        return MarshallHelper.FromStringSet(element);
    }

    AttributeValue IReferenceTypeConverter<IReadOnlySet<string>>.Write(IReadOnlySet<string> element)
    {
        return MarshallHelper.FromStringSet(element);
    }
}