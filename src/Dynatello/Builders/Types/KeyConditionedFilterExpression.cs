using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly struct KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly Func<TReferences, TArgumentReferences, string> Filter;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public KeyConditionedFilterExpression()
    {
        throw Constants.InvalidConstructor();
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