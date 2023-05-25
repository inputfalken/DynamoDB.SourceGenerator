namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

/// <summary>
/// </summary>
public record Settings
{
    public ConsumerMethodConfiguration ConsumerMethodConfig { get; set; } = new("BuildAttributeValues");
    public Keys KeyStrategy { get; set; } = Keys.Include;

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