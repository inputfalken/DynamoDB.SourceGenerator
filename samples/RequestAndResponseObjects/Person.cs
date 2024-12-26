using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;

namespace Dto;

// A typical scenario would be that you would use multiple DynamoDBMarshaller and describe your operations via AccessName.
// If you do not specify an ArgumentType it will use your main entity Type instead which is typically useful for PUT operations.
[DynamoDBMarshaller]
[DynamoDBMarshaller(ArgumentType = typeof((string PersonId, string Firstname)), AccessName = "UpdateFirstName")]
[DynamoDBMarshaller(ArgumentType = typeof(string), AccessName = "GetById")]
public partial class Person
{
    // Will be included as 'PK' in DynamoDB.
    [DynamoDBHashKey("PK")]
    public string Id { get; set; }

    // Will be included as 'Firstname' in DynamoDB.
    public string Firstname { get; set; }

    // Will be included as 'Contact' in DynamoDB.
    [DynamoDBProperty("Contact")]
    public Contact ContactInfo { get; set; }

    // Wont be included in DynamoDB.
    [DynamoDBIgnore]
    public string FirstNameLowercase => Firstname.ToLower();

    public class Contact
    {
        // Will be included as 'Email' in DynamoDB.
        public string Email { get; set; }
    }
}


