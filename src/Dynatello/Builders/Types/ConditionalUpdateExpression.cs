using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly record struct ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;
    internal readonly Func<TReferences, TArgumentReferences, string> Update;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public ConditionalUpdateExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal ConditionalUpdateExpression(
        in TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> update,
        in Func<TReferences, TArgumentReferences, string> condition)
    {
        TableAccess = tableAccess;
        Update = update;
        Condition = condition;
    }
}