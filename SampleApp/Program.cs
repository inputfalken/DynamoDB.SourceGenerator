// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2.Model;

namespace SampleApp
{
    internal static class Program
    {
        public static void Main()
        {
            var entity = new PersonEntity()
            {
                Id = "Abc"
            };


            var PersonEntityDict = new Dictionary<string, AttributeValue>(capacity: 10);
            var ids = new string[] {"abc"};
            if (ids != default)
            {
                var attributeValue = new AttributeValue
                    {L = new List<AttributeValue>(ids.Select(x => new AttributeValue {S = x}))};
                PersonEntityDict.Add("Ids", attributeValue);
            }
//            foreach (var buildAttributeValue in entity.BuildAttributeValues())
//            {
//                Console.WriteLine(buildAttributeValue);
//            }
        }
    }
}