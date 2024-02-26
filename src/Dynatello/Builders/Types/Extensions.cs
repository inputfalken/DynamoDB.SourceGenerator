using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public static class Extensions
{
    public static KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences> WithFilterExpression<T,
        TArg, TReferences, TArgumentReferences>(
        this KeyConditionExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> filter
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences>(
            source.TableAccess,
            source.Condition,
            filter
        );
    }

    public static ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences> WithConditionExpression<T,
        TArg, TReferences, TArgumentReferences>(
        this UpdateExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences>(
            in source.TableAccess,
            in source.Update,
            condition
        );
    }

    public static UpdateExpression<T, TArg, TReferences, TArgumentReferences> WithUpdateExpression<T, TArg, TReferences,
        TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> updateExpression
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new UpdateExpression<T, TArg, TReferences, TArgumentReferences>(in source, in updateExpression);
    }

    public static ConditionExpression<T, TArg, TReferences, TArgumentReferences> WithConditionExpression<T, TArg,
        TReferences,
        TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new ConditionExpression<T, TArg, TReferences, TArgumentReferences>(in source, in condition);
    }

    public static ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences> WithUpdateExpression<T, TArg,
        TReferences,
        TArgumentReferences>(
        this ConditionExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> update
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences>(
            in source.TableAccess,
            in update,
            in source.Condition
        );
    }
}