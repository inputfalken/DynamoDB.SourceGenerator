using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders.Types;
using static DynamoDBGenerator.Extensions.DynamoDBMarshallerExtensions;

namespace Dynatello;

public static class DynamoDBMarshallerExtensions
{
    public static TableAccess<T, TArg, TReferences, TArgumentReferences> OnTable
        <T, TArg, TReferences, TArgumentReferences>
        (this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item, string tableName)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new TableAccess<T, TArg, TReferences, TArgumentReferences>(in tableName, in item);
    }

    internal static Func<TArg, Dictionary<string, AttributeValue>> ComposeKeys<TArg>
    (
        this IDynamoDBKeyMarshaller source,
        Func<TArg, object> partitionKeySelector,
        Func<TArg, object>? rangeKeySelector
    )
    {
        return (partitionKeySelector, rangeKeySelector) switch
        {
            (not null, not null) => y => source.Keys(partitionKeySelector(y), rangeKeySelector(y)),
            (not null, null) => y => source.PartitionKey(partitionKeySelector(y)),
            (null, not null) => y => source.RangeKey(rangeKeySelector(y)),
            (null, null) => throw new ArgumentNullException("")
        };
    }

    internal static Func<TArg, IAttributeExpression> ComposeAttributeExpression<T, TArg, TReferences,
        TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string>? update,
        Func<TReferences, TArgumentReferences, string>? condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return (update, condition) switch
        {
            (null, null) => throw new ArgumentNullException(""),
            (not null, not null) => y => source.ToAttributeExpression(y, update, condition),
            (not null, null) => y => source.ToAttributeExpression(y, update),
            (null, not null) => y => source.ToAttributeExpression(y, condition)
        };
    }
}