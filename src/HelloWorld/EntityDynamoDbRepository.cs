using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;

namespace HelloWorld
{
    public class EntityDynamoDbRepository
    {
        private static IAmazonDynamoDB _dynamoDbClient;

        public EntityDynamoDbRepository(string serviceURL)
        {
            _dynamoDbClient = CreateDynamoDBClient(serviceURL);
        }

        private IAmazonDynamoDB CreateDynamoDBClient(string serviceURL = null)
        {
            if (!string.IsNullOrEmpty(serviceURL))
            {
                var config = new AmazonDynamoDBConfig { ServiceURL = serviceURL };
                return new AmazonDynamoDBClient(config);
            }

            return new AmazonDynamoDBClient();
        }


        private string GenerateUserId()
        {
            return Guid.NewGuid().ToString("n").Substring(0, 8);
        }


        public async Task<List<Entity>> QueryEntitiesByUserAsync(string tableName, string userId)
        {
            var queryRq = new QueryRequest 
            { 
                TableName = tableName,
                KeyConditionExpression = "user_id = :userid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":userid", new AttributeValue(userId) } }
            };
            var queryTask = await _dynamoDbClient.QueryAsync(queryRq);
            var result = queryTask.Items;

            return result.Select(FromDynamoDb).ToList();
        }

        public async Task<Entity> GetItemAsync(string tableName, string userId, string entityId)
        {
            var getitemRq = new GetItemRequest 
            { 
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue> 
                { 
                    { "user_id", new AttributeValue(userId) },
                    { "entity_id", new AttributeValue(entityId) },
                }
            };
            var getitemTask = await _dynamoDbClient.GetItemAsync(getitemRq);
            var result = getitemTask.Item;
            
            if (result.Count > 0)
                return FromDynamoDb(result);
            
            return null;
        }

        // public async Task<List<Entity>> ScanAsync(string tableName)
        // {
        //     var scanRq = new ScanRequest { TableName = tableName };
        //     var scanTask = await _dynamoDbClient.ScanAsync(scanRq);
        //     var result = scanTask.Items;

        //     return result.Select(FromDynamoDb).ToList();
        // }

        public async Task<Entity> PutItemAsync(string tableName, Entity item)
        {
            var dbItem = new Dictionary<string, AttributeValue>();
            if (string.IsNullOrEmpty(item.Id))
                item.Id = GenerateUserId();

            dbItem.Add("user_id", new AttributeValue(item.UserId));
            dbItem.Add("entity_id", new AttributeValue(item.Id));

            dbItem.Add("entity_name", new AttributeValue(item.Name));

            await _dynamoDbClient.PutItemAsync(tableName, dbItem);
            return item;
        }

        public async Task<int> DeleteItemAsync(string tableName, Entity item)
        {
            var dbItem = new Dictionary<string, AttributeValue>();
            dbItem.Add("user_id", new AttributeValue(item.UserId));
            dbItem.Add("entity_id", new AttributeValue(item.Id));

            var result = await _dynamoDbClient.DeleteItemAsync(tableName, dbItem, ReturnValue.ALL_OLD);
            return result.Attributes.Count;
        }

        private Entity FromDynamoDb(Dictionary<string, AttributeValue> item)
        {
            var result = new Entity();
            result.UserId = item["user_id"].S;
            result.Id = item["entity_id"].S;
            result.Name = item["entity_name"].S;
            return result;
        }
    }
}
