using DynamoDBGenerator.SourceGenerator.Types;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

/// <summary>
/// </summary>
public record Settings(string SourceGeneratedClassName)
{
    public ConsumerMethodConfiguration ConsumerMethodConfig { get; set; } = new("BuildAttributeValues");

    public Keys KeyStrategy { get; set; } = Keys.Include;

    // TODO instead of making the the consumer have the ability to set the class name we could just offer some form of identifier.
    // The main reason this exist is to make the consumer be able to use this generation multiple times without collision.
    // We could return a type that contains the AccessPattern (SourceGenerated_Class_X) as well as the method name.
    public string SourceGeneratedClassName { get; } = SourceGeneratedClassName;

    public enum Keys
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

    public record ConsumerMethodConfiguration(in string Name)
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
        public Constants.AccessModifier AccessModifier { get; set; } = Constants.AccessModifier.Public;


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
}