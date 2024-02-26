using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly struct KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly Func<TReferences, TArgumentReferences, string> Filter;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;

    [Obsolete("Do not use this constructor!", true)]
    public KeyConditionedFilterExpression()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    internal KeyConditionedFilterExpression(
        in TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition,
        in Func<TReferences, TArgumentReferences, string> filter
    )
    {
        TableAccess = tableAccess;
        Condition = condition;
        Filter = filter;
    }
}

public readonly struct KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;

    [Obsolete("Do not use this constructor!", true)]
    public KeyConditionExpression()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    internal KeyConditionExpression(
        in TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition
    )
    {
        TableAccess = tableAccess;
        Condition = condition;
    }
}
