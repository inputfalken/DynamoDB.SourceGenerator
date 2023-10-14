using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
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
    {

        var nameTracker = item.AttributeNameExpressionTracker();
        var valueTracker = item.AttributeExpressionValueTracker();
        var expressions = Expressions(nameTracker, valueTracker, expressionBuilders).ToArray();

        return new AttributeExpression(
            Expressions: expressions,
            Values: CreateDictionary(valueTracker.AccessedValues(arg)),
            Names: CreateDictionary(nameTracker.AccessedNames())
        );

        static Dictionary<string, TValue> CreateDictionary<TValue>(IEnumerable<KeyValuePair<string, TValue>> keyValuePairs)
        {
            var dict = new Dictionary<string, TValue>();
            foreach (var keyValuePair in keyValuePairs)
                dict[keyValuePair.Key] = keyValuePair.Value;

            return dict;
        }

        static IEnumerable<string> Expressions(TReferences references, TArgumentReferences argumentReferences, IEnumerable<Func<TReferences, TArgumentReferences, string>> expressionBuilders)
        {
            foreach (var expressionBuilder in expressionBuilders)
                yield return expressionBuilder(references, argumentReferences);
        }

    }
}
