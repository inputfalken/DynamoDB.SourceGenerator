using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

// DynamoDB
public static class DynamoDBMarshallerExtensions
{
    public static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
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

    public static PutItemRequest ToPutItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg entity,
        string tableName,
        Func<TReferences, TArgumentReferences, string>? conditionExpressionBuilder = null)
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
        where TArg : T
    {

        Dictionary<string, string>? names = null;
        Dictionary<string, AttributeValue>? values = null;
        string? expression = null;
        if (conditionExpressionBuilder is not null)
        {
            var nameTracker = item.AttributeNameExpressionTracker();
            var argumentTracker = item.AttributeExpressionValueTracker();
            expression = conditionExpressionBuilder(nameTracker, argumentTracker);
            names = nameTracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value);
            values = argumentTracker.AccessedValues(entity).ToDictionary(x => x.Key, x => x.Value);
        }

        return new PutItemRequest
        {
            TableName = tableName,
            Item = item.Marshall(entity),
            ExpressionAttributeNames = names,
            ExpressionAttributeValues = values,
            ConditionExpression = expression
        };
    }
}