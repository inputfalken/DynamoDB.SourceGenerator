using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly record struct ConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;

    [Obsolete("Do not use this constructor!", true)]
    public ConditionExpression()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    internal ConditionExpression(
        in TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition
    )
    {
        TableAccess = tableAccess;
        Condition = condition;
    }
}