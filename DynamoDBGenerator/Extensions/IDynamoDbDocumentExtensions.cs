using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

public static class DynamoDbDocumentExtensions
{
    public static PutItemRequest CreatePutItemRequest<T, TReferences>(this IDynamoDbDocument<T, TReferences> item, T entity, Func<TReferences, string>? conditionExpression = null)
    {
        var conditionAttributeExpression = conditionExpression is not null ? item.ConditionExpression(conditionExpression) : null;
        return new PutItemRequest
        {
            Item = item.Marshal(entity),
            ExpressionAttributeNames = conditionAttributeExpression?.Names(),
            ExpressionAttributeValues = conditionAttributeExpression?.Values(entity),
            ConditionExpression = conditionAttributeExpression?.Expression
        };
    }

    public static UpdateItemRequest CreateUpdateItemRequest<T, TReferences>(
        this IDynamoDbDocument<T, TReferences> item,
        T entity,
        Func<TReferences, string> updateExpression,
        Func<TReferences, string>? conditionExpression = null)
    {
        var conditionAttributeExpression = conditionExpression is not null ? item.ConditionExpression(conditionExpression) : null;
        var updateAttributeExpression = item.UpdateExpression(updateExpression);
        
        
        return new UpdateItemRequest()
        {
            Key = item.Keys(entity),
            ExpressionAttributeNames = conditionAttributeExpression?.Names().Concat(updateAttributeExpression.Names()).ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = conditionAttributeExpression?.Values(entity),
            ConditionExpression = conditionAttributeExpression?.Expression,
            UpdateExpression = updateAttributeExpression.Expression,
        };
    }

}