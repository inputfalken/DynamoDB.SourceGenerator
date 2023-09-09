using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator;

public interface IDynamoDBClient<TEntity, TArgument, out TReferences, out TArgumentReferences>
    where TReferences : IExpressionAttributeNameTracker
    where TArgumentReferences : IExpressionAttributeValueTracker<TArgument>
{
    Task Save<T>(
        T entity,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    ) where T : TEntity, TArgument;
    Task Save<T>(
        T entity,
        CancellationToken cancellationToken = default
    ) where T : TEntity, TArgument;
    Task Update(
        TArgument entity,
        Func<IDynamoDBKeyMarshaller, TArgument, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        CancellationToken cancellationToken = default
    );
    Task Update(
        TArgument entity,
        Func<IDynamoDBKeyMarshaller, TArgument, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    );

    Task<TEntity> UpdateReturned(
        TArgument entity,
        Func<IDynamoDBKeyMarshaller, TArgument, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    );
}

internal class DynamoDBClient<T, TArg, TReferences, TArgumentReferences> : IDynamoDBClient<T, TArg, TReferences, TArgumentReferences> where TReferences : IExpressionAttributeNameTracker
    where TArgumentReferences : IExpressionAttributeValueTracker<TArg>
{
    private readonly IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> _marshaller;
    private readonly string _tableName;
    private readonly IAmazonDynamoDB _amazonDynamoDB;

    public DynamoDBClient(IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> marshaller, string tableName, IAmazonDynamoDB amazonDynamoDB)
    {
        _marshaller = marshaller;
        _tableName = tableName;
        _amazonDynamoDB = amazonDynamoDB;
    }

    public Task Save<T1>(T1 entity, Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder, CancellationToken cancellationToken = default) where T1 : T, TArg
    {
        var putRequest = _marshaller.ToPutItemRequestInternal(entity, entity, conditionExpressionBuilder, ReturnValue.NONE, _tableName);
        return _amazonDynamoDB.PutItemAsync(putRequest, cancellationToken);
    }

    public Task Save<T1>(T1 entity, CancellationToken cancellationToken = default) where T1 : T, TArg
    {
        var putRequest = _marshaller.ToPutItemRequestInternal(entity, entity, null, ReturnValue.NONE, _tableName);
        return _amazonDynamoDB.PutItemAsync(putRequest, cancellationToken);
    }
    public Task Update(
        TArg entity,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        CancellationToken cancellationToken = default
    )
    {
        var updateItemRequest = _marshaller.ToUpdateItemRequestInternal(entity, keySelector, updateExpressionBuilder, null, ReturnValue.NONE, _tableName);
        return _amazonDynamoDB.UpdateItemAsync(updateItemRequest, cancellationToken);
    }
    public Task Update(
        TArg entity,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    )
    {
        var updateItemRequest = _marshaller.ToUpdateItemRequestInternal(entity, keySelector, updateExpressionBuilder, conditionExpressionBuilder, ReturnValue.NONE, _tableName);
        return _amazonDynamoDB.UpdateItemAsync(updateItemRequest, cancellationToken);
    }
    public async Task<T> UpdateReturned(TArg entity, Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector, Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder, CancellationToken cancellationToken = default)
    {
        var updateItemRequest = _marshaller.ToUpdateItemRequestInternal(entity, keySelector, updateExpressionBuilder, conditionExpressionBuilder, ReturnValue.ALL_NEW, _tableName);
        var result = await _amazonDynamoDB.UpdateItemAsync(updateItemRequest, cancellationToken);

        return _marshaller.Unmarshall(result.Attributes);
    }
}

public class DynamoDBMarshallingException : InvalidOperationException
{
    public string MemberName { get; }
    public DynamoDBMarshallingException(string memberName, string message) : base(message: $"{message} (Data member '{memberName}')")
    {
        MemberName = memberName;
    }
}