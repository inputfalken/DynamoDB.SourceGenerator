using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

public static class DynamoDBMarshallerExtensions
{
    public static IDynamoDBClient<T, TArg, TReferences, TArgumentReferences> ToDynamoDBClient<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        string tableName,
        IAmazonDynamoDB dynamoDB
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg> => new DynamoDBClient<T, TArg, TReferences, TArgumentReferences>(item, tableName, dynamoDB);
    public static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg> => item.ToUpdateItemRequestInternal(argument, keySelector, updateExpressionBuilder, null, returnValue, tableName);

    public static UpdateItemRequest ToUpdateItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        TArg argument,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        ReturnValue returnValue,
        string tableName
    )
        where TReferences : IExpressionAttributeNameTracker
        where TArgumentReferences : IExpressionAttributeValueTracker<TArg> => item.ToUpdateItemRequestInternal(argument, keySelector, updateExpressionBuilder, conditionExpressionBuilder, returnValue, tableName);

    public static PutItemRequest ToPutItemRequest<T, TArg, TReferences, TArgumentReferences>(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item,
        T entity,
        ReturnValue returnValue,
        string tableName
    )
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