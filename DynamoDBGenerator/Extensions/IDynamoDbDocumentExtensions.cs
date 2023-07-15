using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

public static class DynamoDbDocumentExtensions
{
    public static PutItemRequest ToPutItemRequest<T, TReferences>(
        this IDynamoDbDocument<T, TReferences> item,
        T entity,
        string tableName,
        Func<TReferences, string>? conditionExpressionBuilder = null) where TReferences : IExpressionAttributeReferences<T>
    {

        Dictionary<string, string>? names = null;
        Dictionary<string, AttributeValue>? values = null;
        string? expression = null;
        if (conditionExpressionBuilder is not null)
        {
            var tracker = item.ExpressionAttributeTracker();
            expression = conditionExpressionBuilder(tracker);
            names = tracker.AccessedNames().ToDictionary(x => x.Key, x => x.Value);
            values = tracker.AccessedValues(entity).ToDictionary(x => x.Key, x => x.Value);
        }
        
        return new PutItemRequest
        {
            TableName = tableName,
            Item = item.Serialize(entity),
            ExpressionAttributeNames = names,
            ExpressionAttributeValues = values,
            ConditionExpression = expression
        };
    }
}