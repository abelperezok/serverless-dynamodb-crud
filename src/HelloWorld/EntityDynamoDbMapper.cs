using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace AwsDynamoDb
{
    public class EntityDynamoDbMapper : IDynamoDbMapper
    {
        public Dictionary<string,object> FromDynamoDb(Dictionary<string, AttributeValue> item)
        {
            var result = new Dictionary<string, object>();
            result.Add("Id", item["Id"].N);
            result.Add("Name", item["Name"].S);
            return result;
        }

        public Dictionary<string,AttributeValue> ToDynamoDb(Dictionary<string,object> item)
        {
            // map generic dictionary to DynamoDb attribute values
            var result = new Dictionary<string, AttributeValue>();
            if (item.ContainsKey("Id")) 
                result.Add("Id", new AttributeValue { N = Convert.ToString(item["Id"]) });
            result.Add("Name", new AttributeValue { S = Convert.ToString(item["Name"]) });
            return result;
        }
    }
}