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

            var f = new CSharpToAttributeValue.Generation(namedTypeSymbol);

            yield return f.CreateExpressionAttributeNames();
        }
    }
}