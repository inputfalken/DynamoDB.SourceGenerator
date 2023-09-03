using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

// DynamoDB
public static class DynamoDBMarshallerExtensions
{
    public static IDynamoDBClient<T, TArg, TReferences, TArgumentReferences> ToDynamoDBClient<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        string tableName,
        IAmazonDynamoDB dynamoDB
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
    {
        return new DynamoDBClient<T, TArg, TReferences, TArgumentReferences>(item, tableName, dynamoDB);

    }
    public static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        string tableName,
        Func<IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences>, TArg, Dictionary<string, AttributeValue>> keySelector,
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
            Key = keySelector(item, argument),
            TableName = tableName,
            ExpressionAttributeNames = nameTracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = argumentTracker.AccessedValues(argument).ToDictionary(x => x.Key, x => x.Value),
            ConditionExpression = conditionExpression,
            UpdateExpression = updateExpression
        };
    }

    public static PutItemRequest ToPutItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        ReturnValue returnValue,
        string tableName)
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
        where T : TArg => item.ToPutItemRequestInternal(entity, entity, null, returnValue, tableName);

    public static PutItemRequest ToPutItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
        where T : TArg => item.ToPutItemRequestInternal(entity, entity, conditionExpressionBuilder, returnValue, tableName);


}