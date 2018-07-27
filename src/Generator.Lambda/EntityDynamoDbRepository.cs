using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using Generator.Domain;

namespace Generator.Lambda
{

    public class EntityDynamoDbRepository : IEntityRepository
    {
        private static IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName;

        public EntityDynamoDbRepository(string tableName, string serviceURL = null)
        {
            _tableName = tableName;
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


        public async Task<List<Entity>> GetEntitiesByUserAsync(string userId)
        {
            var queryRq = new QueryRequest
            {
                TableName = _tableName,
                KeyConditionExpression = "user_id = :userid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":userid", new AttributeValue(userId) } }
            };
            var queryTask = await _dynamoDbClient.QueryAsync(queryRq);
            var result = queryTask.Items;

            return result.Select(FromDynamoDb).ToList();
        }

        public async Task<Entity> GetItemAsync(string userId, string entityId)
        {
            var getitemRq = new GetItemRequest
            {
                TableName = _tableName,
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

        public async Task<Entity> PutItemAsync(Entity item)
        {
            var dbItem = new Dictionary<string, AttributeValue>();
            if (string.IsNullOrEmpty(item.Id))
                item.Id = GenerateUserId();

            dbItem.Add("user_id", new AttributeValue(item.UserId));
            dbItem.Add("entity_id", new AttributeValue(item.Id));

            dbItem.Add("entity_name", new AttributeValue(item.Name));

            await _dynamoDbClient.PutItemAsync(_tableName, dbItem);
            return item;
        }

        public async Task<int> DeleteItemAsync(Entity item)
        {
            var dbItem = new Dictionary<string, AttributeValue>();
            dbItem.Add("user_id", new AttributeValue(item.UserId));
            dbItem.Add("entity_id", new AttributeValue(item.Id));

            var result = await _dynamoDbClient.DeleteItemAsync(_tableName, dbItem, ReturnValue.ALL_OLD);
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
