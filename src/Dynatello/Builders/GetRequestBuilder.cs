using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Builders;

public readonly record struct GetRequestBuilder<T>
{
    private readonly Func<T, Dictionary<string, AttributeValue>> _keysSelector;

    internal GetRequestBuilder(
        string tableName,
        Func<T, Dictionary<string, AttributeValue>> keysSelector)
    {
        _keysSelector = keysSelector;
        TableName = tableName;
    }

    [Obsolete("Do not used this constructor!", true)]
    public GetRequestBuilder()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    /// <inheritdoc cref="GetItemRequest.TableName" />
    public string TableName { get; init; }

    /// <inheritdoc cref="GetItemRequest.ConsistentRead" />
    public bool? ConsistentRead { get; init; } = null;

    /// <inheritdoc cref="GetItemRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    public GetItemRequest Build(T arg)
    {
        var request = new GetItemRequest
        {
            ReturnConsumedCapacity = ReturnConsumedCapacity,
            TableName = TableName,
            Key = _keysSelector(arg)
        };

        if (ConsistentRead is { } consistentRead)
            request.ConsistentRead = consistentRead;

        if (ReturnConsumedCapacity is not null)
            request.ReturnConsumedCapacity = ReturnConsumedCapacity;

        return request;
    }
}