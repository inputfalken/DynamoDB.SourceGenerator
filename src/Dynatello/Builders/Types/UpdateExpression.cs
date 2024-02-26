using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly record struct UpdateExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly TableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess;
    internal readonly Func<TReferences, TArgumentReferences, string> Update;

    [Obsolete("Do not use this constructor!", true)]
    public UpdateExpression()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    internal UpdateExpression(
        in TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> update)
    {
        TableAccess = tableAccess;
        Update = update;
    }
}