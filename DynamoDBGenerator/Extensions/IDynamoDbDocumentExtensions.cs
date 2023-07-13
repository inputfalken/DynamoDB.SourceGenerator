using System;
using System.Linq;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Extensions;

public static class DynamoDbDocumentExtensions
{
    public static PutItemRequest ToPutItemRequest<T, TReferences>(
        this IDynamoDbDocument<T, TReferences> item,
        T entity,
        string tableName,
        Func<TReferences, string>? conditionExpression = null)
    {
        var conditionAttributeExpression = conditionExpression is not null ? item.ConditionExpression(conditionExpression) : null;
        return new PutItemRequest
        {
            TableName = tableName,
            Item = item.Serialize(entity),
            ExpressionAttributeNames = conditionAttributeExpression?.References.AccessedNames().ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = conditionAttributeExpression?.References.AccessedValues(entity).ToDictionary(x => x.Key, x => x.Value),
            ConditionExpression = conditionAttributeExpression?.Expression
        };
    }
}