using Amazon.DynamoDBv2;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        ProductRepository productRepository = new ProductRepository("MY_TABLE", new AmazonDynamoDBClient());
        
    }
}