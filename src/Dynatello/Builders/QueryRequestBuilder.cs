using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace Dynatello.Builders;

public readonly record struct QueryRequestBuilder<T>
{
    private readonly Func<T, IAttributeExpression> _attributeExpressionSelector;

    internal QueryRequestBuilder(Func<T, IAttributeExpression> attributeExpressionSelector, string tableName)
    {
        _attributeExpressionSelector = attributeExpressionSelector;
        TableName = tableName;
    }

    [Obsolete("Do not used this constructor!", true)]
    public QueryRequestBuilder()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    /// <inheritdoc cref="QueryRequest.TableName" />
    public string TableName { get; init; }

    /// <inheritdoc cref="QueryRequest.IndexName" />
    public string? IndexName { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.Limit" />
    public int? Limit { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.ConsistentRead" />
    public bool? ConsistentRead { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.ScanIndexForward" />
    public bool? ScanIndexForward { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.Select" />
    public Select? Select { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    public QueryRequest Build(T arg)
    {
        var attributeExpression = _attributeExpressionSelector(arg);

        var queryRequest = new QueryRequest
        {
            AttributesToGet = null,
            QueryFilter = null,
            ConditionalOperator = null,
            KeyConditions = null,
            KeyConditionExpression = attributeExpression.Expressions[0],
            ExpressionAttributeValues = attributeExpression.Values,
            ExpressionAttributeNames = attributeExpression.Names,
            TableName = TableName,
            IndexName = IndexName,
            ProjectionExpression = null
        };


        if (ReturnConsumedCapacity is not null)
            queryRequest.ReturnConsumedCapacity = ReturnConsumedCapacity;

        if (ConsistentRead is { } consistentRead)
            queryRequest.ConsistentRead = consistentRead;

        if (ScanIndexForward is { } scanIndexForward)
            queryRequest.ScanIndexForward = scanIndexForward;

        if (Select is not null)
            queryRequest.Select = Select;

        if (Limit is { } limit)
            queryRequest.Limit = limit;

        if (IndexName is not null)
            queryRequest.IndexName = IndexName;

        if (attributeExpression.Expressions.Count == 2)
            queryRequest.FilterExpression = attributeExpression.Expressions[1];

        return queryRequest;
    }
}