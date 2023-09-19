using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

/// <summary>
/// Represents a client with asynchronous methods for sending requests to DynamoDB.
/// </summary>
/// <typeparam name="TEntity">The type of entity associated with DynamoDB operations.</typeparam>
/// <typeparam name="TArgument">The type of argument used in DynamoDB operations.</typeparam>
/// <typeparam name="TReferences">The type for tracking attribute names related to <typeparamref name="TEntity"/>.</typeparam>
/// <typeparam name="TArgumentReferences">The type for tracking argument attribute values related to <typeparamref name="TArgument"/>.</typeparam>
public interface IDynamoDBClient<TEntity, TArgument, out TReferences, out TArgumentReferences>
    where TReferences : IExpressionAttributeNameTracker
    where TArgumentReferences : IExpressionAttributeValueTracker<TArgument>
{
    /// <summary>
    /// Saves an entity to DynamoDB with an optional condition expression builder.
    /// </summary>
    /// <typeparam name="T">The type of entity to save.</typeparam>
    /// <param name="entity">The entity to be saved.</param>
    /// <param name="conditionExpressionBuilder">A function to build a condition expression.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task Save<T>(
        T entity,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    ) where T : TEntity, TArgument;

    /// <summary>
    /// Saves an entity to DynamoDB without a condition expression.
    /// </summary>
    /// <typeparam name="T">The type of entity to save.</typeparam>
    /// <param name="entity">The entity to be saved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task Save<T>(
        T entity,
        CancellationToken cancellationToken = default
    ) where T : TEntity, TArgument;

    /// <summary>
    /// Updates an entity in DynamoDB.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="keySelector">A function to select the keys for the update operation.</param>
    /// <param name="updateExpressionBuilder">A function to build the update expression.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task Update(
        TArgument entity,
        Func<IDynamoDBKeyMarshaller, TArgument, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates an entity in DynamoDB with optional condition and update expression builders.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="keySelector">A function to select the keys for the update operation.</param>
    /// <param name="updateExpressionBuilder">A function to build the update expression.</param>
    /// <param name="conditionExpressionBuilder">A function to build the condition expression.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task Update(
        TArgument entity,
        Func<IDynamoDBKeyMarshaller, TArgument, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates an entity in DynamoDB and returns the updated entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="keySelector">A function to select the keys for the update operation.</param>
    /// <param name="updateExpressionBuilder">A function to build the update expression.</param>
    /// <param name="conditionExpressionBuilder">A function to build the condition expression.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateReturned(
        TArgument entity,
        Func<IDynamoDBKeyMarshaller, TArgument, Dictionary<string, AttributeValue>> keySelector,
        Func<TReferences, TArgumentReferences, string> updateExpressionBuilder,
        Func<TReferences, TArgumentReferences, string> conditionExpressionBuilder,
        CancellationToken cancellationToken = default
    );
}