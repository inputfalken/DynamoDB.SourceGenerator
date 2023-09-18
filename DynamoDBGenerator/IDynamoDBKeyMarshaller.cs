using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

// TODO might be good to include a option to Get the keys based on a generic argument which would be TArg from IDynamoDBMarshaller. Then the consumer could customize their DynamoDBKeyAttributes on the TArg payload. This would not work for tuples though. 😢
public interface IDynamoDBKeyMarshaller
{
    /// <summary>
    ///     Creates <see cref="Dictionary{TKey,TValue}" /> from the fields attributed with
    ///     <see cref="DynamoDBHashKeyAttribute" /> and or cref="DynamoDBRangeKeyAttribute"/>.
    /// </summary>
    public Dictionary<string, AttributeValue> Keys(object partitionKey, object rangeKey);
    /// <summary>
    ///     Creates <see cref="Dictionary{TKey,TValue}" /> from the fields attributed with
    ///     <see cref="DynamoDBHashKeyAttribute" />
    /// </summary>
    public Dictionary<string, AttributeValue> PartitionKey(object key);

    /// Creates
    /// <see cref="Dictionary{TKey,TValue}" />
    /// from the fields attributed with
    /// <see cref="DynamoDBRangeKeyAttribute" />
    public Dictionary<string, AttributeValue> RangeKey(object key);
}