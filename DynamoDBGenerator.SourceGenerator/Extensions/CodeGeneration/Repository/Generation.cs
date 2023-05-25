using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue.Settings.
    ConsumerMethodConfiguration.Parameterization;

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
                var settings = new Settings
                {
                    ConsumerMethodConfig =
                        new Settings.ConsumerMethodConfiguration($"Put{namedTypeSymbol.Name}AttributeValues")
                        {
                            MethodParameterization = ParameterizedInstance
                        }
                };
                var conversion = namedTypeSymbol.GeneratePocoToAttributeValueFactory(in settings);
                yield return conversion.Code;
            }


            if (attributeClassName is nameof(DynamoDBUpdateOperationAttribute))
            {
                var keysSettings = new Settings
                {
                    KeyStrategy = Settings.Keys.Only,
                    ConsumerMethodConfig =
                        new Settings.ConsumerMethodConfiguration($"Update{namedTypeSymbol.Name}AttributeValueKeys")
                        {
                            MethodParameterization = ParameterizedInstance
                        }
                };
                var keys = namedTypeSymbol.GeneratePocoToAttributeValueFactory(in keysSettings);
                yield return keys.Code;

                var settings = new Settings
                {
                    KeyStrategy = Settings.Keys.Ignore,
                    ConsumerMethodConfig =
                        new Settings.ConsumerMethodConfiguration($"Update{namedTypeSymbol.Name}AttributeValues")
                        {
                            MethodParameterization = ParameterizedInstance
                        }
                };
                var conversion = namedTypeSymbol.GeneratePocoToAttributeValueFactory(in settings);
                yield return conversion.Code;
            }
        }
    }
}