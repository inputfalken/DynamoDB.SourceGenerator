using System;
using System.Collections.Generic;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/>
/// </summary>
public static class DynamoDBMarshallerExtensions
{
    /// <summary>
    /// Creates an <see cref="IAttributeExpression"/> based on the expressions being built inside <paramref name="expressionBuilders"/>
    /// The expression can be accessed in the same order as you passed arguments to <paramref name="expressionBuilders"/>.
    /// </summary>
    public static IAttributeExpression ToAttributeExpression<TArg, TReferences, TArgumentReferences>(
        Func<TReferences> entityReferences,
        Func<TArgumentReferences> argumentReferences,
        TArg arg,
        params Func<TReferences, TArgumentReferences, string>[] expressionBuilders
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        var nameTracker = entityReferences();
        var valueTracker = argumentReferences();

        var expressions = Expressions(nameTracker, valueTracker, expressionBuilders);

        return new AttributeExpression(
            Expressions: expressions,
            Values: CreateDictionary(valueTracker.AccessedValues(arg)),
            Names: CreateDictionary(nameTracker.AccessedNames())
        );

        static Dictionary<string, TValue> CreateDictionary<TValue>(
            IEnumerable<KeyValuePair<string, TValue>> keyValuePairs)
        {
            var dict = new Dictionary<string, TValue>();
            foreach (var keyValuePair in keyValuePairs)
                dict[keyValuePair.Key] = keyValuePair.Value;

            return dict;
        }

        static string[] Expressions(
            TReferences references,
            TArgumentReferences argumentReferences,
            Func<TReferences, TArgumentReferences, string>[] expressionBuilders)
        {
            var arr = new string[expressionBuilders.Length];
            for (var i = 0; i < expressionBuilders.Length; i++) 
                arr[i] = expressionBuilders[i](references, argumentReferences);

            return arr;
        }
    }

    /// <inheritdoc cref="ToAttributeExpression{TArg,TReferences,TArgumentReferences}"/>
    public static IAttributeExpression ToAttributeExpression<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg arg,
        params Func<TReferences, TArgumentReferences, string>[] expressionBuilders
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return ToAttributeExpression(
            item.AttributeExpressionNameTracker,
            item.AttributeExpressionValueTracker,
            arg,
            expressionBuilders
        );
    }
}