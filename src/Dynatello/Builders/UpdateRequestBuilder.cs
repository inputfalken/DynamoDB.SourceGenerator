using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace Dynatello.Builders;

/// <summary>
///     A record based builder for creating <see cref="UpdateItemRequest" /> that can be configured via the `with` syntax.
/// </summary>
public readonly record struct UpdateRequestBuilder<T>
{
    private readonly Func<T, IAttributeExpression> _attributeExpressionSelector;
    private readonly IDynamoDBKeyMarshaller _keyMarshaller;
    private readonly Func<IDynamoDBKeyMarshaller, T, Dictionary<string, AttributeValue>> _keySelector;

    private readonly string _tableName;

    [Obsolete("Do not used this constructor!", true)]
    public UpdateRequestBuilder()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    internal UpdateRequestBuilder(
        Func<T, IAttributeExpression> attributeExpressionSelector,
        string tableName,
        Func<IDynamoDBKeyMarshaller, T, Dictionary<string, AttributeValue>> keySelector,
        IDynamoDBKeyMarshaller keyMarshaller
    )
    {
        _attributeExpressionSelector = attributeExpressionSelector;
        _tableName = tableName;
        _keySelector = keySelector;
        _keyMarshaller = keyMarshaller;
    }

    /// <inheritdoc cref="PutItemRequest.TableName"/>
    public string TableName
    {
        get => _tableName;
        init => _tableName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     A function to specify how the keys should be accessed through the <typeparamref name="T" />.
    /// </summary>
    public Func<IDynamoDBKeyMarshaller, T, Dictionary<string, AttributeValue>> KeySelector
    {
        get => _keySelector;
        init => _keySelector = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc cref="UpdateItemRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    /// <inheritdoc cref="UpdateItemRequest.ReturnItemCollectionMetrics" />
    public ReturnItemCollectionMetrics? ReturnItemCollectionMetrics { get; init; } = null;

    /// <inheritdoc cref="UpdateItemRequest.ReturnValues" />
    public ReturnValue? ReturnValues { get; init; } = null;

    /// <inheritdoc cref="UpdateItemRequest.ReturnValuesOnConditionCheckFailure" />
    public ReturnValuesOnConditionCheckFailure? ReturnValuesOnConditionCheckFailure { get; init; } = null;


    /// <summary>
    ///     Will build a <see cref="UpdateItemRequest" /> with the specified configurations.
    /// </summary>
    public UpdateItemRequest Build(T arg)
    {
        var expression = _attributeExpressionSelector(arg);
        var update = new UpdateItemRequest
        {
            UpdateExpression = expression.Expressions[0],
            ConditionExpression = expression.Expressions.Count is 2 ? expression.Expressions[1] : null,
            TableName = TableName,
            Key = KeySelector(_keyMarshaller, arg),
            ExpressionAttributeNames = expression.Names,
            ExpressionAttributeValues = expression.Values,
            Expected = null,
            AttributeUpdates = null,
            ConditionalOperator = null
        };

        if (ReturnValues is not null)
            update.ReturnValues = ReturnValues;

        if (ReturnConsumedCapacity is not null)
            update.ReturnConsumedCapacity = ReturnConsumedCapacity;

        if (ReturnItemCollectionMetrics is not null)
            update.ReturnItemCollectionMetrics = ReturnItemCollectionMetrics;

        if (ReturnValuesOnConditionCheckFailure is not null)
            update.ReturnValuesOnConditionCheckFailure = ReturnValuesOnConditionCheckFailure;


        return update;
    }
}