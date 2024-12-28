// See https://aka.ms/new-console-template for more information

using System.Net;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Extensions;
using RequestAndResponseObjects;

static GetItemRequest CreateGetItemRequest()
{
    return new GetItemRequest
    {
        Key = Person.GetById.PrimaryKeyMarshaller.PartitionKey("123"),
        TableName = "MyTable"
    };
}

static Person DeserializeResponse(GetItemResponse response)
{
    if (response.HttpStatusCode != HttpStatusCode.OK)
        throw new NotImplementedException();
    
    return Person.GetById.Unmarshall(response.Item);
}

static PutItemRequest PutPerson()
{
    return new PutItemRequest
    {
        TableName = "MyTable",
        Item = Person.PersonMarshaller.Marshall(new Person
        {
            Firstname = "John",
            Id = Guid.NewGuid().ToString(),
            ContactInfo = new Person.Contact { Email = "john@test.com" }
        })
    };
}

static UpdateItemRequest UpdateFirstName()
{
    // Creating an AttributeExpression can be done through string interpolation where the source generator will mimic your DTO types and give you an consistent API to build the attributeExpressions.
    var attributeExpression = Person.UpdateFirstName.ToAttributeExpression(
        ("personId", "John"),
        (dbRef, argRef) => $"{dbRef.Id} = {argRef.PersonId}", // The condition
        (dbRef, argRef) => $"SET {dbRef.Firstname} = {argRef.Firstname}" // The update operation
    );

// the index can be used to retrieve the expressions in the same order as you provide the string interpolations in the method call above.
    var condition = attributeExpression.Expressions[0];
    var update = attributeExpression.Expressions[1];
    var keys = Person.UpdateFirstName.PrimaryKeyMarshaller.PartitionKey("personId");

    return new UpdateItemRequest
    {
        ConditionExpression = condition,
        UpdateExpression = update,
        ExpressionAttributeNames = attributeExpression.Names,
        ExpressionAttributeValues = attributeExpression.Values,
        Key = keys,
        TableName = "MyTable"
    };
}