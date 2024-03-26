using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Builders.Types;

ProductRepository productRepository = new ProductRepository("MY_TABLE", new AmazonDynamoDBClient());

public class ProductRepository
{
    private readonly IAmazonDynamoDB _amazonDynamoDb;
    private readonly GetRequestBuilder<string> _getProductByTable;
    private readonly UpdateRequestBuilder<(string Id, decimal NewPrice, DateTime TimeStamp)> _updatePrice;
    private readonly PutRequestBuilder<Product> _createProduct;
    private readonly QueryRequestBuilder<decimal> _queryByPrice;

    public ProductRepository(string tableName, IAmazonDynamoDB amazonDynamoDb)
    {
        _amazonDynamoDb = amazonDynamoDb;

        _getProductByTable = Product.GetById
            .OnTable(tableName)
            .ToGetRequestBuilder(arg => arg); // Since the ArgumentType is set to string, we don't need to select a property.

        _updatePrice = Product.UpdatePrice
            .OnTable(tableName)
            .WithUpdateExpression((db, arg) => $"SET {db.Price} = {arg.NewPrice}, {db.Metadata.ModifiedAt} = {arg.TimeStamp}") // Specify the update operation
            .ToUpdateItemRequestBuilder((marshaller, arg) => marshaller.PartitionKey(arg.Id));

        _createProduct = Product.Put
            .OnTable(tableName)
            .WithConditionExpression((db, arg) => $"{db.Id} <> {arg.Id}") // Ensure we don't have an existing Product in DynamoDB
            .ToPutRequestBuilder();

        _queryByPrice = Product.QueryByPrice
                .OnTable(tableName)
                .WithKeyConditionExpression((db, arg) => $"{db.Price} = {arg}")
                .ToQueryRequestBuilder() 
            with
            {
                IndexName = Product.PriceIndex
            };
    }

    public async Task<IReadOnlyList<Product>> SearchByPrice(decimal price)
    {
        QueryRequest request = _queryByPrice.Build(price);
        QueryResponse? response = await _amazonDynamoDb.QueryAsync(request);

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            throw new Exception("...");

        return response.Items
            .Select(x => Product.QueryByPrice.Unmarshall(x))
            .ToArray();
    }

    public async Task Create(Product product)
    {
        PutItemRequest request = _createProduct.Build(product);
        PutItemResponse response = await _amazonDynamoDb.PutItemAsync(request);

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            throw new Exception("...");
    }

    public async Task<Product?> GetById(string id)
    {
        GetItemRequest request = _getProductByTable.Build(id);
        GetItemResponse response = await _amazonDynamoDb.GetItemAsync(request);

        if (response.HttpStatusCode is HttpStatusCode.NotFound)
            return null;

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            throw new Exception("...");

        Product product = Product.GetById.Unmarshall(response.Item);

        return product;
    }

    public async Task<Product?> UpdatePrice(string id, decimal price)
    {
        UpdateItemRequest request = _updatePrice.Build((id, price, DateTime.UtcNow));
        UpdateItemResponse response = await _amazonDynamoDb.UpdateItemAsync(request);

        if (response.HttpStatusCode is not HttpStatusCode.OK)
            return null;

        Product product = Product.UpdatePrice.Unmarshall(response.Attributes);

        return product;
    }
}

[DynamoDBMarshaller(typeof(Product), PropertyName = "Put")]
[DynamoDBMarshaller(typeof(Product), ArgumentType = typeof(string), PropertyName = "GetById")]
[DynamoDBMarshaller(typeof(Product), ArgumentType = typeof((string Id, decimal NewPrice, DateTime TimeStamp)), PropertyName = "UpdatePrice")]
[DynamoDBMarshaller(typeof(Product), ArgumentType = typeof(decimal), PropertyName = "QueryByPrice")]
public partial record Product(
    [property: DynamoDBHashKey, DynamoDBGlobalSecondaryIndexRangeKey(Product.PriceIndex)] string Id,
    [property: DynamoDBGlobalSecondaryIndexHashKey(Product.PriceIndex)] decimal Price,
    string Description,
    Product.MetadataEntity Metadata
)
{
    public const string PriceIndex = "PriceIndex";

    public record MetadataEntity(DateTime CreatedAt, DateTime ModifiedAt);
}
