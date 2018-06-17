using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace AwsDynamoDb
{
    public interface IDynamoDbMapper
    {
        Dictionary<string,object> FromDynamoDb(Dictionary<string, AttributeValue> item);

        Dictionary<string,AttributeValue> ToDynamoDb(Dictionary<string,object> item);
    }
}