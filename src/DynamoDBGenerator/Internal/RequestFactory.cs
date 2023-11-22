using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Internal;

internal static class RequestFactory
{
    internal static PutItemRequest ToPutItemRequestInternal<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        TArg argument,
        Func<TReferences, TArgumentReferences, string>? conditionExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {

        Dictionary<string, AttributeValue>? expressionAttributeValues = null;
        Dictionary<string, string>? expressionAttributeNames = null;
        string? conditionExpression = null;

        if (conditionExpressionBuilder is not null)
        {
            var nameTracker = item.AttributeExpressionNameTracker();
            var valueTracker = item.AttributeExpressionValueTracker();
            conditionExpression = conditionExpressionBuilder.Invoke(nameTracker, valueTracker);
            expressionAttributeNames = nameTracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value);
            expressionAttributeValues = valueTracker.AccessedValues(argument).ToDictionary(x => x.Key, x => x.Value);
        }

        return new PutItemRequest
        {
            TableName = tableName,
            ExpressionAttributeNames = expressionAttributeNames,
            ExpressionAttributeValues = expressionAttributeValues,
            ConditionExpression = conditionExpression,
            Item = item.Marshall(entity),
            ReturnValues = returnValue
        };
    }
    internal static UpdateItemRequest ToUpdateItemRequestInternal<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string>? conditionExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {

        var nameTracker = item.AttributeExpressionNameTracker();
        var argumentTracker = item.AttributeExpressionValueTracker();
        var updateExpression = updateExpressionBuilder(nameTracker, argumentTracker);
        var conditionExpression = conditionExpressionBuilder?.Invoke(nameTracker, argumentTracker);

        return new UpdateItemRequest
        {
            Key = keySelector(item.PrimaryKeyMarshaller, argument),
            TableName = tableName,
            ExpressionAttributeNames = nameTracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = argumentTracker.AccessedValues(argument).ToDictionary(x => x.Key, x => x.Value),
            ConditionExpression = conditionExpression,
            UpdateExpression = updateExpression,
            ReturnValues = returnValue
        };
    }
}