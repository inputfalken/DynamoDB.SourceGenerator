using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly struct KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public KeyConditionExpression()
    {
        throw Constants.InvalidConstructor();
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