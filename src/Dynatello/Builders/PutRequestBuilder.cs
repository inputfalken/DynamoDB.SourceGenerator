using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace Dynatello.Builders;

/// <summary>
/// A <see cref="PutItemRequest"/> builder that can be configured through the record `with` syntax.
/// </summary>
/// <typeparam name="T">
/// The type you need to provide in you execution.
/// </typeparam>
public readonly record struct PutRequestBuilder<T>
{
    private readonly Func<T, IAttributeExpression>? _attributeExpressionSelector;
    private readonly Func<T, Dictionary<string, AttributeValue>> _marshall;

    private readonly string _tableName;


    internal PutRequestBuilder(
        Func<T, IAttributeExpression>? attributeExpressionSelector,
        Func<T, Dictionary<string, AttributeValue>> marshall,
        string tableName
    )
    {
        _attributeExpressionSelector = attributeExpressionSelector;
        _marshall = marshall;
        _tableName = tableName;
    }

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public PutRequestBuilder()
    {
        throw Constants.InvalidConstructor();
    }

    /// <inheritdoc cref="PutItemRequest.TableName" />
    public string TableName
    {
        get => _tableName;
        init => _tableName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc cref="PutItemRequest.ReturnValues" />
    public ReturnValue? ReturnValues { get; init; } = null;

    /// <inheritdoc cref="PutItemRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    /// <inheritdoc cref="PutItemRequest.ReturnItemCollectionMetrics" />
    public ReturnItemCollectionMetrics? ReturnItemCollectionMetrics { get; init; } = null;

    /// <inheritdoc cref="PutItemRequest.ReturnValuesOnConditionCheckFailure" />
    public ReturnValuesOnConditionCheckFailure? ReturnValuesOnConditionCheckFailure { get; init; } = null;


    public PutItemRequest Build(T element)
    {
        var request = new PutItemRequest
        {
            TableName = TableName,
            Item = _marshall(element),
            Expected = null,
            ConditionalOperator = null,
            ConditionExpression = null,
            ExpressionAttributeNames = null,
            ExpressionAttributeValues = null
        };

        if (ReturnValues is not null)
            request.ReturnValues = ReturnValues;
        if (ReturnConsumedCapacity is not null)
            request.ReturnConsumedCapacity = ReturnConsumedCapacity;
        if (ReturnItemCollectionMetrics is not null)
            request.ReturnItemCollectionMetrics = ReturnItemCollectionMetrics;
        if (ReturnValuesOnConditionCheckFailure is not null)
            request.ReturnValuesOnConditionCheckFailure = ReturnValuesOnConditionCheckFailure;

        if (_attributeExpressionSelector is null) return request;
        var attributeExpression = _attributeExpressionSelector(element);

        request.ExpressionAttributeNames = attributeExpression.Names;
        request.ExpressionAttributeValues = attributeExpression.Values;
        request.ConditionExpression = attributeExpression.Expressions[0];

        return request;
    }
}