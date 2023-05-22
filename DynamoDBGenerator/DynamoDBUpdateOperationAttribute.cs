using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator;

// ReSharper disable once InconsistentNaming
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DynamoDBUpdateOperationAttribute : Attribute
{
    public Type? Type { get; }

    public DynamoDBUpdateOperationAttribute(Type type)
    {
        Type = type;
    }
}

//    public UpdateItemRequest Request = new UpdateItemRequest
//    {
//        TableName = _tableName,
//        // Solved
//        Key = new Dictionary<string, AttributeValue>
//            {
//                {PartitionKey, new AttributeValue {S = determinedReplacement.OrderId}},
//                {SortKey, new AttributeValue {S = determinedReplacement.OrderedProduct.ProductId}}
//            },
//        // Not Supported
//        ExpressionAttributeNames =
//            new Dictionary<string, string>
//            {
//                {"#S", nameof(ReplacementEntity.Status)},
//                {"#DT", nameof(ReplacementEntity.Metadata.StatusModifiedAt)},
//                {"#MD", nameof(ReplacementEntity.Metadata)},
//            },
//        // Could be solved if were able to perform a Functor operation on the DDBAttributeName together with the existing functionality.
//        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
//        {
//            {":status", new AttributeValue {S = determinedReplacement.Status}},
//            {":statusUpdatedAt", new AttributeValue {S = DateTimeOffset.UtcNow.ToIso8601()}}
//        },
//        ConditionExpression = "#S <> :status",
//        UpdateExpression = "SET #S = :status, #MD.#DT = :statusUpdatedAt",
//        ReturnValues = ReturnValue.NONE
//    };
//}