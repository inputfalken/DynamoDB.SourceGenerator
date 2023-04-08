// See https://aka.ms/new-console-template for more information

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        var entity = new PersonEntity()
        {
            Id = "Abc"
        };

        foreach (var buildAttributeValue in entity.BuildAttributeValues())
        {
            Console.WriteLine(buildAttributeValue);
        }

    }
}