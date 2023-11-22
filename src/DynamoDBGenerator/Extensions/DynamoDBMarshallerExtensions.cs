using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
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
    public static IAttributeExpression ToAttributeExpression<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg arg,
        params Func<TReferences, TArgumentReferences, string>[] expressionBuilders
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {

        var nameTracker = item.AttributeExpressionNameTracker();
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

    /// <summary>
    /// Converts the <see cref="IDynamoDBMarshaller{TEntity,TArg,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/> into an <see cref="IDynamoDBClient{TEntity,TArgument,TReferences,TArgumentReferences}"/>.
    /// </summary>
    public static IDynamoDBClient<T, TArg, TReferences, TArgumentReferences> ToDynamoDBClient<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        string tableName,
        IAmazonDynamoDB dynamoDB
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new DynamoDBClient<T, TArg, TReferences, TArgumentReferences>(item, tableName, dynamoDB);
    }

    /// <summary>
    /// Creates a <see cref="PutItemRequest"/>.
    /// </summary>
    public static PutItemRequest ToPutItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where T : TArg
    {
        return item.ToPutItemRequestInternal(entity, entity, null, returnValue, tableName);
    }

    /// <summary>
    /// Creates a <see cref="PutItemRequest"/> with condition expression.
    /// </summary>
    public static PutItemRequest ToPutItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where T : TArg
    {
        return item.ToPutItemRequestInternal(entity, entity, conditionExpressionBuilder, returnValue, tableName);
    }

    /// <summary>
    /// Creates a <see cref="UpdateItemRequest"/>.
    /// </summary>
    public static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return item.ToUpdateItemRequestInternal(argument, keySelector, updateExpressionBuilder, null, returnValue, tableName);
    }

    /// <summary>
    /// Creates a <see cref="UpdateItemRequest"/> with a condition expression.
    /// </summary>
    public static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return item.ToUpdateItemRequestInternal(argument, keySelector, updateExpressionBuilder, conditionExpressionBuilder, returnValue, tableName);
    }
}