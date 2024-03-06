using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Builders.Types;

namespace SampleApp;

public class ProductRepository
{
    private readonly IAmazonDynamoDB _amazonDynamoDb;
    private readonly GetRequestBuilder<string> _getProductByTable;
    private readonly UpdateRequestBuilder<(string Id, decimal NewPrice, DateTime TimeStamp)> _updatePrice;
    private readonly PutRequestBuilder<ProductEntity> _createProduct;
    private readonly QueryRequestBuilder<decimal> _queryByPrice;

    public ProductRepository(string tableName, IAmazonDynamoDB amazonDynamoDb)
    {
        _amazonDynamoDb = amazonDynamoDb;

        _getProductByTable = ProductEntity.GetById
            .OnTable(tableName)
            .ToGetRequestBuilder(argumentType => argumentType); // Since the ArgumentType is set to string, we don't need to select a property.

        _updatePrice = ProductEntity.UpdatePrice
                .OnTable(tableName)
                .WithUpdateExpression((x, y) => $"SET {x.Price} = {y.NewPrice}, {x.Metadata.ModifiedAt} = {y.TimeStamp}") // Specify the update operation
                .ToUpdateItemRequestBuilder((x, y) => x.PartitionKey(y.Id))
            with
            {
                ReturnValues = ReturnValue.ALL_NEW
            };

        _createProduct = ProductEntity.PutProduct
            .OnTable(tableName)
            .WithConditionExpression((x, y) => $"{x.Id} <> {y.Id}") // Ensure we don't have an existing Product in DynamoDB
            .ToPutRequestBuilder();

        _queryByPrice = ProductEntity.QueryByPrice
                .OnTable(tableName)
                .WithKeyConditionExpression((x, y) => $"{x.Price} = {y}")
                .ToQueryRequestBuilder() with
            {
                IndexName = ProductEntity.PriceIndex
            };
    }

    public async Task<IReadOnlyList<ProductEntity>> SearchByPrice(decimal price)
    {
        QueryRequest request = _queryByPrice.Build(price);
        QueryResponse? response = await _amazonDynamoDb.QueryAsync(request);

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            throw new Exception("...");

        return response.Items
            .Select(x => ProductEntity.QueryByPrice.Unmarshall(x))
            .ToArray();
    }

    public async Task Create(ProductEntity productEntity)
    {
        PutItemRequest request = _createProduct.Build(productEntity);
        PutItemResponse response = await _amazonDynamoDb.PutItemAsync(request);

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            throw new Exception("...");
    }

    public async Task<ProductEntity?> GetById(string id)
    {
        GetItemRequest request = _getProductByTable.Build(id);
        GetItemResponse response = await _amazonDynamoDb.GetItemAsync(request);

        if (response.HttpStatusCode is HttpStatusCode.NotFound)
            return null;

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            throw new Exception("...");

        ProductEntity productEntity = ProductEntity.GetById.Unmarshall(response.Item);

        return productEntity;
    }

    public async Task<ProductEntity?> UpdatePrice(string id, decimal price)
    {
        UpdateItemRequest request = _updatePrice.Build((id, price, DateTime.UtcNow));
        UpdateItemResponse response = await _amazonDynamoDb.UpdateItemAsync(request);

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            return null;

        ProductEntity productEntity = ProductEntity.UpdatePrice.Unmarshall(response.Attributes);

        return productEntity;
    }
}

[DynamoDBMarshaller(typeof(ProductEntity), PropertyName = "PutProduct")]
[DynamoDBMarshaller(typeof(ProductEntity), ArgumentType = typeof(string), PropertyName = "GetById")]
[DynamoDBMarshaller(typeof(ProductEntity), ArgumentType = typeof((string Id, decimal NewPrice, DateTime TimeStamp)), PropertyName = "UpdatePrice")]
[DynamoDBMarshaller(typeof(ProductEntity), ArgumentType = typeof(decimal), PropertyName = "QueryByPrice")]
public partial record ProductEntity(
    [property: DynamoDBHashKey] string Id,
    [property: DynamoDBGlobalSecondaryIndexHashKey(ProductEntity.PriceIndex)]
    decimal Price,
    string Description,
    ProductEntity.MetadataEntity Metadata
)
{
    public const string PriceIndex = "PriceIndex";

    public record MetadataEntity(DateTime CreatedAt, DateTime ModifiedAt);
}