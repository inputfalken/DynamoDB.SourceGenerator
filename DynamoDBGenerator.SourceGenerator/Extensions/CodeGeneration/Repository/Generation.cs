using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.Repository;

public static class Generation
{
    public static string CreateRepository(this ITypeSymbol typeSymbol, Compilation compilation)
    {
        return string.Join(Constants.NewLine, GetMethods(typeSymbol, compilation));
    }

    private static IEnumerable<string> GetMethods(ITypeSymbol typeSymbol, Compilation compilation)
    {
        var typeNames = typeSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass?.ContainingNamespace is {Name: nameof(DynamoDBGenerator)})
            .Where(x => x.AttributeClass!.Name is nameof(DynamoDBPutOperationAttribute)
                or nameof(DynamoDBUpdateOperationAttribute))
            .SelectMany(x => x.ConstructorArguments, (x, y) => (AttributeData: x, TypeConstant: y))
            .Where(x => x.TypeConstant.Kind is TypedConstantKind.Type);

        foreach (var typeName in typeNames)
        {
            var namedTypeSymbol = compilation.GetBestTypeByMetadataName(typeName.TypeConstant.Value!.ToString());
            if (namedTypeSymbol is null)
                continue;


            var attributeClassName = typeName.AttributeData.AttributeClass!.Name;
            if (attributeClassName is nameof(DynamoDBPutOperationAttribute))
            {
                var settings = new Settings(
                    namedTypeSymbol.Name,
                    new Settings.ConsumerMethodConfiguration($"Put{namedTypeSymbol.Name}AttributeValues", Settings.ConsumerMethodConfiguration.Parameterization.ParameterizedInstance, Constants.AccessModifier.Public),
                    null,
                    $"SourceGenerated_{namedTypeSymbol.Name}_Put_Conversion"
                );
                var conversion = namedTypeSymbol.GenerateAttributeValueConversion(in settings);
                yield return conversion;
            }


            if (attributeClassName is nameof(DynamoDBUpdateOperationAttribute))
            {
                var keysSettings = new Settings(
                    namedTypeSymbol.Name,
                    new Settings.ConsumerMethodConfiguration($"Update{namedTypeSymbol.Name}AttributeValueKeys", Settings.ConsumerMethodConfiguration.Parameterization.ParameterizedInstance, Constants.AccessModifier.Public),
                    new Settings.PredicateConfiguration(static x => x.IsHashKey || x.IsRangeKey),
                    $"SourceGenerated_{namedTypeSymbol.Name}_Update_Key_Conversion"
                );
                var keys = namedTypeSymbol.GenerateAttributeValueConversion(in keysSettings);
                yield return keys;
                
                var settings = new Settings(
                    namedTypeSymbol.Name,
                    new Settings.ConsumerMethodConfiguration($"Update{namedTypeSymbol.Name}AttributeValues", Settings.ConsumerMethodConfiguration.Parameterization.ParameterizedInstance, Constants.AccessModifier.Public),
                    new Settings.PredicateConfiguration(static x => x.IsHashKey is false && x.IsRangeKey is false),
                    $"SourceGenerated_{namedTypeSymbol.Name}_Update_Conversion"
                );
                var conversion = namedTypeSymbol.GenerateAttributeValueConversion(in settings);
                yield return conversion;
                
            }
        }
    }
}