using System;
using System.Collections.Generic;
using System.Linq;
using DynamoDBGenerator.Internal;
namespace DynamoDBGenerator.Extensions;

public static class ExpressionAttributeExtensions
{
    /// <summary>
    /// Creates an <see cref="IAttributeExpression"/> based on the expressions being built inside <paramref name="expressionBuilders"/>
    /// The expression can be accessed in the same order as you passed arguments to <paramref name="expressionBuilders"/>.
    /// </summary>
    public static IAttributeExpression ToAttributeExpression<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg arg,
        params Func<TReferences, TArgumentReferences, string>[] expressionBuilders
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
        where T : TArg
    {

        var nameTracker = item.AttributeNameExpressionTracker();
        var valueTracker = item.AttributeExpressionValueTracker();
        var expressions = Expressions(nameTracker, valueTracker, expressionBuilders).ToArray();

        return new AttributeExpression(
            Expressions: expressions,
            Values: valueTracker.AccessedValues(arg).ToDictionary(x => x.Key, x => x.Value),
            Names: nameTracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value)
        );

        static IEnumerable<string> Expressions(TReferences references, TArgumentReferences argumentReferences, IEnumerable<Func<TReferences, TArgumentReferences, string>> expressionBuilders)
        {
            foreach (var expressionBuilder in expressionBuilders)
                yield return expressionBuilder(references, argumentReferences);
        }

    }
}