using DynamoDBGenerator;
using Dynatello.Builders.Types;
using static DynamoDBGenerator.Extensions.DynamoDBMarshallerExtensions;

namespace Dynatello;

public static class DynamoDBMarshallerExtensions
{
    public static TableAccess<T, TArg, TReferences, TArgumentReferences> OnTable
        <T, TArg, TReferences, TArgumentReferences>
        (this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item, string tableName)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new TableAccess<T, TArg, TReferences, TArgumentReferences>(in tableName, in item);
    }

    internal static Func<TArg, IAttributeExpression> ComposeAttributeExpression<T, TArg, TReferences,
        TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string>? update,
        Func<TReferences, TArgumentReferences, string>? condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return (update, condition) switch
        {
            (null, null) => throw new ArgumentNullException(""),
            (not null, not null) => y =>
                ToAttributeExpression(
                    source.AttributeExpressionNameTracker,
                    source.AttributeExpressionValueTracker,
                    y,
                    update,
                    condition
                ),
            (not null, null) => y =>
                ToAttributeExpression(
                    source.AttributeExpressionNameTracker,
                    source.AttributeExpressionValueTracker,
                    y,
                    update
                ),
            (null, not null) => y =>
                ToAttributeExpression(
                    source.AttributeExpressionNameTracker,
                    source.AttributeExpressionValueTracker,
                    y,
                    condition
                )
        };
    }
}