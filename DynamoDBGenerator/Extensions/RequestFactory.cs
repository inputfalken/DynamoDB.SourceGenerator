using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

internal static class RequestFactory
{
    internal static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        string tableName,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string>? conditionExpressionBuilder = null
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
    {

        var nameTracker = item.AttributeNameExpressionTracker();
        var argumentTracker = item.AttributeExpressionValueTracker();
        var updateExpression = updateExpressionBuilder(nameTracker, argumentTracker);
        var conditionExpression = conditionExpressionBuilder?.Invoke(nameTracker, argumentTracker);

        return new UpdateItemRequest
        {
            TableName = tableName,
            ExpressionAttributeNames = nameTracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = argumentTracker.AccessedValues(argument).ToDictionary(x => x.Key, x => x.Value),
            ConditionExpression = conditionExpression,
            UpdateExpression = updateExpression
        };
    }
    
    internal static PutItemRequest ToPutItemRequestInternal<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        TArg argument,
        Func<TReferences, TArgumentReferences, string>? conditionExpressionBuilder,
        string tableName
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
    {

        Dictionary<string, AttributeValue>? expressionAttributeValues = null;
        Dictionary<string, string>? expressionAttributeNames = null;
        string? conditionExpression = null;

        if (conditionExpressionBuilder is not null)
        {
            var nameTracker = item.AttributeNameExpressionTracker();
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
            Item = item.Marshall(entity)
        };
    }
}