using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using DynamoDBGenerator.Extensions;
namespace DynamoDBGenerator;

public interface IDynamoDBClient<in TEntity, in TArgument, out TReferences, out TArgumentReferences>
    where TReferences : IExpressionAttributeNameTracker
    where TArgumentReferences : IExpressionAttributeValueTracker<TArgument>
{
    Task PutItemAsync<T>(T entity, Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder, CancellationToken cancellationToken = default) where T : TEntity, TArgument;
    Task PutItemAsync<T>(T entity, CancellationToken cancellationToken = default) where T : TEntity, TArgument;
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

    public Task PutItemAsync<T1>(T1 entity, Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder, CancellationToken cancellationToken = default) where T1 : T, TArg
    {
        var putRequest = _marshaller.ToPutItemRequestInternal(entity, entity, conditionExpressionBuilder, ReturnValue.NONE, _tableName);

        return _amazonDynamoDB.PutItemAsync(putRequest, cancellationToken);
    }

    public Task PutItemAsync<T1>(T1 entity, CancellationToken cancellationToken = default) where T1 : T, TArg
    {
        var putRequest = _marshaller.ToPutItemRequestInternal(entity, entity, null, ReturnValue.NONE, _tableName);

        return _amazonDynamoDB.PutItemAsync(putRequest, cancellationToken);
    }
}