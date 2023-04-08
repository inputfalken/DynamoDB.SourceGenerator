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

//            var name = PersonEntity.AttributeValueKeys.Name;
//            entity.BuildAttributeValues();
            //entity.BuildAttributeValues();
        }
//            foreach (var buildAttributeValue in entity.BuildAttributeValues())
//            {
//                Console.WriteLine(buildAttributeValue);
//            }
        }
    }