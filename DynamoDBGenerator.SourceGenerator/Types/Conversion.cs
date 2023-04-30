namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct Conversion<TFrom, TTowards>
{
    public TFrom From { get; }
    public TTowards Towards { get; }

    public Conversion(in TFrom from, in TTowards towards)
    {
        From = from;
        Towards = towards;
    }
}