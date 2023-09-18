using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
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