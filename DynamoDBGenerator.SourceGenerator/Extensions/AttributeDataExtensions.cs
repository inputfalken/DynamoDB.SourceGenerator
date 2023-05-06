using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class AttributeDataExtensions
{

    public static T? CreateInstance<T>(this AttributeData attributeData) where T : Attribute
    {
        if (attributeData.AttributeClass?.Name != typeof(T).Name) return null;

        return (T) Activator.CreateInstance(
            typeof(T),
            attributeData.ConstructorArguments.Select(z =>
            {
                if (z.Kind is TypedConstantKind.Type)
                    throw new NotSupportedException(
                        $"Constructor argument of {nameof(Type)} can not be resolved for attributes.");
                return z.Value;
            }).ToArray());
    }
}