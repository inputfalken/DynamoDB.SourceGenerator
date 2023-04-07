namespace DynamoDBGenerator;

[AttributeUsage(AttributeTargets.Class)]
public class AttributeValueGeneratorAttribute : Attribute
{
    public AttributeValueGeneratorAttribute()
    {
    }
}