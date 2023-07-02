using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

public static class DynamoDbDocumentExtensions
{
    public static PutItemRequest CreatePutItemRequest<T, TReferences>(this IDynamoDbDocument<T, TReferences> item, T entity, Func<TReferences, string>? conditionExpression = null)
    {
        AttributeExpression<T>? conditionAttributeExpression = conditionExpression is not null ? item.ConditionExpression(conditionExpression) : null;
        return new PutItemRequest
        {
            Item = item.Marshal(entity),
            ExpressionAttributeNames = conditionAttributeExpression.Value.ExpressionAttributeReferences.AccessedNames().ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = conditionAttributeExpression.Value.ExpressionAttributeReferences.AccessedValues(entity).ToDictionary(x => x.Key, x => x.Value),
        };
    }


}