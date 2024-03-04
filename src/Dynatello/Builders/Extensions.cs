using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders.Types;

namespace Dynatello.Builders;

public static class Extensions
{
    public static QueryRequestBuilder<TArg> ToQueryRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new QueryRequestBuilder<TArg>(
            source.TableAccess.Item.ComposeAttributeExpression(source.Condition, source.Filter),
            source.TableAccess.TableName
        );
    }

    public static QueryRequestBuilder<TArg> ToQueryRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this KeyConditionExpression<T, TArg, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new QueryRequestBuilder<TArg>(
            source.TableAccess.Item.ComposeAttributeExpression(source.Condition, null),
            source.TableAccess.TableName
        );
    }

    public static GetRequestBuilder<TArg> ToGetRequestBuilder<T, TArg, TReferences, TArgumentReferences,
        TPartition>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> source,
        Func<TArg, TPartition> partitionKeySelector)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where TPartition : notnull
    {
        return new GetRequestBuilder<TArg>(
            source.TableName,
            source.Item.PrimaryKeyMarshaller.ComposeKeys<TArg>(y => partitionKeySelector(y), null)
        );
    }

    public static GetRequestBuilder<TArg> ToGetRequestBuilder<T, TArg,
        TReferences, TArgumentReferences, TPartition, TRange>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> source,
        Func<TArg, TPartition> partitionKeySelector,
        Func<TArg, TRange> rangeKeySelector)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where TPartition : notnull
        where TRange : notnull
    {
        return new GetRequestBuilder<TArg>(
            source.TableName,
            source.Item.PrimaryKeyMarshaller.ComposeKeys<TArg>(y => partitionKeySelector(y), y => rangeKeySelector(y))
        );
    }


    public static UpdateRequestBuilder<TArg> ToUpdateItemRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this UpdateExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new UpdateRequestBuilder<TArg>(
            source.TableAccess.Item.ComposeAttributeExpression(source.Update, null),
            source.TableAccess.TableName,
            keySelector,
            source.TableAccess.Item.PrimaryKeyMarshaller
        );
    }

    public static UpdateRequestBuilder<TArg> ToUpdateItemRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new UpdateRequestBuilder<TArg>(
            source.TableAccess.Item.ComposeAttributeExpression(source.Update, source.Condition),
            source.TableAccess.TableName,
            keySelector,
            source.TableAccess.Item.PrimaryKeyMarshaller
        );
    }

    public static KeyConditionExpression<T, TArg, TReferences, TArgumentReferences> WithKeyConditionExpression<T, TArg,
        TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>(source, condition);
    }

    public static PutRequestBuilder<T> ToPutRequestBuilder<T, TReferences,
        TArgumentReferences>(
        this TableAccess<T, T, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<T>
    {
        return new PutRequestBuilder<T>
        (
            null,
            source.Item.Marshall,
            source.TableName
        );
    }

    public static PutRequestBuilder<T> ToPutRequestBuilder<T, TReferences,
        TArgumentReferences>(
        this ConditionExpression<T, T, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<T>
    {
        return new PutRequestBuilder<T>
        (
            source.TableAccess.Item.ComposeAttributeExpression(null, source.Condition),
            source.TableAccess.Item.Marshall,
            source.TableAccess.TableName
        );
    }
}