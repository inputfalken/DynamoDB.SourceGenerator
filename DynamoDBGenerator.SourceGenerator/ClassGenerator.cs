using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

// TODO will generate an a method signature similar to:
// public static {ClassWithGeneratorAttribute} BuildClass(IReadOnlyDictionary<string, AttributeValue> attributeValues) { ... }
// This method is expected to be interchangeable with AttributeValueGenerator.
public class ClassGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        throw new NotImplementedException();
    }
}