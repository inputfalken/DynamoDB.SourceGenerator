// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2.Model;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        var valueTuple = (foo: 1, bar: 2);
        var f = valueTuple.GetType();

        foreach (var memberInfo in f.GetMembers())
        {

            Console.WriteLine(memberInfo.Name);
        }
    }
}