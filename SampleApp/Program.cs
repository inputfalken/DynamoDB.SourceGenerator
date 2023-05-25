// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
    }
}

[DynamoDBUpdateOperation(typeof(Address))]
public partial class Repository
{
}