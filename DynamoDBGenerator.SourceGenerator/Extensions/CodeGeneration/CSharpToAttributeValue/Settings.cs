using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public enum KeyStrategy
{
    /// <summary>
    /// Include keys and everything else.
    /// </summary>
    Include = 1,

    /// <summary>
    /// Ignore DynamoDB key properties.
    /// </summary>
    Ignore = 2,

    /// <summary>
    /// Only include DynamoDB key properties.
    /// </summary>
    Only = 3,
}

public record MethodConfiguration(in string Name)
{
    /// <summary>
    ///     The name method.
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    ///     Determines how to invoke the method. 
    /// </summary>
    public Parameterization MethodParameterization { get; set; } = Parameterization.UnparameterizedInstance;

    /// <summary>
    /// Determines the access modifier.
    /// </summary>
    public Accessibility AccessModifier { get; set; } = Accessibility.Public;


    public enum Parameterization
    {
        /// <summary>
        /// Will make the method be be unparameterized by expecting the generation occur from the current instance.
        /// </summary>
        UnparameterizedInstance = 1,

        /// <summary>
        /// Will make the method be invoked from a static fashion by expecting the type to be the param.
        /// </summary>
        ParameterizedStatic = 2,

        /// <summary>
        ///  Will make the method be invoked from in a instance fashion by expecting the type to be the param.
        /// </summary>
        ParameterizedInstance
    }
}