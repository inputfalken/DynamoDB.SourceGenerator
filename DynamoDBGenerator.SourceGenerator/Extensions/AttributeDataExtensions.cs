using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class AttributeDataExtensions
{
    public static T? CreateInstance<T>(this AttributeData attributeData) where T : Attribute
    {
        if (attributeData.AttributeClass?.Name != typeof(T).Name) return null;

        return (T) Activator.CreateInstance(
            typeof(T),
            attributeData.ConstructorArguments.Select(z => z.Value).ToArray());
    }
}