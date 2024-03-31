using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with a condition.
/// </summary>
public readonly record struct ConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public ConditionExpression()
    {
        throw Constants.InvalidConstructor();
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