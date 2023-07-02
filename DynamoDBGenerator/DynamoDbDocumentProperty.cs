using System;

namespace DynamoDBGenerator
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DynamoDbDocumentProperty : Attribute
    {
        public DynamoDbDocumentProperty(Type type)
        {

        }
    }
}