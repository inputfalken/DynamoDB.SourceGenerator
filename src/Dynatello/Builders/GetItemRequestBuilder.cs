using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace Dynatello.Builders;

public readonly record struct GetItemRequestBuilder<TKey> where TKey : notnull
{
    private readonly IDynamoDBKeyMarshaller _keyMarshaller;
    private readonly Func<IDynamoDBKeyMarshaller, TKey, Dictionary<string, AttributeValue>> _keysSelector;

    /// <inheritdoc cref="GetItemRequest.TableName"/>
    public string TableName { get; init; }


    /// <inheritdoc cref="GetItemRequest.ConsistentRead"/>
    public bool? ConsistentRead { get; init; } = null;

    /// <inheritdoc cref="GetItemRequest.ReturnConsumedCapacity"/>
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    internal GetItemRequestBuilder(string tableName, IDynamoDBKeyMarshaller keyMarshaller,
        Func<IDynamoDBKeyMarshaller, TKey, Dictionary<string, AttributeValue>> keysSelector)
    {
        _keyMarshaller = keyMarshaller;
        _keysSelector = keysSelector;
        TableName = tableName;
    }

    public GetItemRequest Build(TKey key)
    {
        var request = new GetItemRequest
        {
            ReturnConsumedCapacity = ReturnConsumedCapacity,
            TableName = TableName,
            Key = _keysSelector(_keyMarshaller, key)
        };

        if (ConsistentRead is { } consistentRead)
            request.ConsistentRead = consistentRead;

        if (ReturnConsumedCapacity is not null)
            request.ReturnConsumedCapacity = ReturnConsumedCapacity;

        return request;
    }
}