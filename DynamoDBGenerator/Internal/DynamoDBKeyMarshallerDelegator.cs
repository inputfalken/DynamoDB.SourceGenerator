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
public sealed class DynamoDBKeyMarshallerDelegator : IDynamoDBKeyMarshaller

{
    private readonly Func<object?, object?, bool, bool, string?, Dictionary<string, AttributeValue>> _fn;
    public DynamoDBKeyMarshallerDelegator(Func<object?, object?, bool, bool, string?, Dictionary<string, AttributeValue>> fn) => _fn = fn;
    public Dictionary<string, AttributeValue> Keys(object partitionKey, object rangeKey) => _fn(partitionKey, rangeKey, true, true, null);
    public Dictionary<string, AttributeValue> PartitionKey(object key) => _fn(key, null, true, false, null);
    public Dictionary<string, AttributeValue> RangeKey(object key) => _fn(null, key, false, true, null);
}