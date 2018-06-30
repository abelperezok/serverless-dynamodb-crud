using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

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

        public async Task<List<Entity>> ScanAsync(string tableName)
        {
            var scanRq = new ScanRequest { TableName = tableName };
            var scanTask = await _dynamoDbClient.ScanAsync(scanRq);
            var result = scanTask.Items;

            return result.Select(FromDynamoDb).ToList();
        }

        public async Task PutItemAsync(string tableName, Entity item)
        {
            var dbItem = new Dictionary<string, AttributeValue>();
            dbItem.Add("user_id", new AttributeValue(item.UserId));
            dbItem.Add("entity_id", new AttributeValue(item.Id));
            dbItem.Add("entity_name", new AttributeValue(item.Name));

            await _dynamoDbClient.PutItemAsync(tableName, dbItem);
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
