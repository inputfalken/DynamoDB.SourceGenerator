using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly record struct UpdateExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;
    internal readonly Func<TReferences, TArgumentReferences, string> Update;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public UpdateExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal UpdateExpression(
        in TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> update)
    {
        TableAccess = tableAccess;
        Update = update;
    }
}