using DynamoDBGenerator.SourceGenerator.Types;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

/// <summary>
/// </summary>
public readonly record struct Settings(
    in Settings.ConsumerMethodConfiguration ConsumerMethodConfig,
    Settings.PredicateConfiguration? PredicateConfig,
    string SourceGeneratedClassName
    )
{
    public ConsumerMethodConfiguration ConsumerMethodConfig { get; } = ConsumerMethodConfig;

    public PredicateConfiguration? PredicateConfig { get; } = PredicateConfig;

    // TODO instead of making the the consumer have the ability to set the class name we could just offer some form of identifier.
    // The main reason this exist is to make the consumer be able to use this generation multiple times without collision.
    // We could return a type that contains the AccessPattern (SourceGenerated_Class_X) as well as the method name.
    public string SourceGeneratedClassName { get; } = SourceGeneratedClassName;

    public readonly record struct PredicateConfiguration(in Func<DynamoDbDataMember, bool> Predicate)
    {
        /// <summary>
        /// A recursive predicate that will be applied according.
        /// </summary>
        public Func<DynamoDbDataMember, bool> Predicate { get; } = Predicate;
        
    }
    public readonly record struct ConsumerMethodConfiguration(in string Name, ConsumerMethodConfiguration.Parameterization MethodParameterization, Constants.AccessModifier AccessModifier)
    {
        /// <summary>
        ///     The name method.
        /// </summary>
        public string Name { get; } = Name;

        /// <summary>
        ///     Determines how to invoke the method. 
        /// </summary>
        public Parameterization MethodParameterization { get; } = MethodParameterization;

        /// <summary>
        /// Determines the access modifier.
        /// </summary>
        public Constants.AccessModifier AccessModifier { get; } = AccessModifier;
        
        
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