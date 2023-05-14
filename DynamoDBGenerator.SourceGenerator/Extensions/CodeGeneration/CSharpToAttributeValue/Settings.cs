using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Types;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

/// <summary>
/// </summary>
public readonly record struct Settings(
    in string MPropertyMethodName,
    in Settings.ConsumerMethodConfiguration ConsumerMethodConfig,
    Settings.PredicateConfiguration? PredicateConfig,
    string SourceGeneratedClassName
    )
{
    /// <summary>
    ///     The generated method name that will be used for  <see cref="AttributeValue" /> property
    ///     <see cref="AttributeValue.M" />.
    /// </summary>
    public string MPropertyMethodName { get; } = MPropertyMethodName;

    public ConsumerMethodConfiguration ConsumerMethodConfig { get; } = ConsumerMethodConfig;

    public PredicateConfiguration? PredicateConfig { get; } = PredicateConfig;

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