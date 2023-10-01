using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Internal;

/// <summary>
///     This is an internal API that supports the source generator and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new version.
/// </summary>
public sealed class IndexDynamoDBMarshallerDelegator : IDynamoDBIndexKeyMarshaller
{
    private readonly Func<object?, object?, bool, bool, string?, Dictionary<string, AttributeValue>> _fn;
    public IndexDynamoDBMarshallerDelegator(Func<object?, object?, bool, bool, string?, Dictionary<string, AttributeValue>> fn, string index)
    {
        _fn = fn;
        Index = index;
    }
    public Dictionary<string, AttributeValue> Keys(object partitionKey, object rangeKey) => _fn(partitionKey, rangeKey, true, true, Index);
    public Dictionary<string, AttributeValue> PartitionKey(object key) => _fn(key, null, true, false, Index);
    public Dictionary<string, AttributeValue> RangeKey(object key) => _fn(null, key, false, true, Index);
    public string Index { get; }
}