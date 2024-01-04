using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

//    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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