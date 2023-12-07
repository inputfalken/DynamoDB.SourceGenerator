using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

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
    public static AttributeValue? ToAttributeValue(Dictionary<string, AttributeValue>? dict)
    {
        return dict is null ? null : new() {M = dict};
    }
#pragma warning restore CS1591
}