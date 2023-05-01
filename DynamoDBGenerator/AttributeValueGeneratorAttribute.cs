using System;

namespace DynamoDBGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AttributeValueGeneratorAttribute : Attribute
    {
        public const string DefaultMethodName = "BuildAttributeValues";
        public string MethodName { get; }

        public AttributeValueGeneratorAttribute(string methodName = DefaultMethodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));
            
            MethodName = methodName;
        }
    }
}