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
    // TODO enrich IDynamoDBKeyMarshaller to contain the intent that was invoked. Such whether Keys, PartitionKey, RangeKey was used.
    private readonly Func<object?, object?, bool, bool, string?, Dictionary<string, AttributeValue>> _implementation;
    public IndexDynamoDBMarshallerDelegator(Func<object?, object?, bool, bool, string?, Dictionary<string, AttributeValue>> implementation, string index)
    {
        _implementation = implementation;
        Index = index;
    }
    public Dictionary<string, AttributeValue> Keys(object partitionKey, object rangeKey) => _implementation(partitionKey, rangeKey, true, true, Index);
    public Dictionary<string, AttributeValue> PartitionKey(object key) => _implementation(key, null, true, false, Index);
    public Dictionary<string, AttributeValue> RangeKey(object key) => _implementation(null, key, false, true, Index);
    public string Index { get; }
}