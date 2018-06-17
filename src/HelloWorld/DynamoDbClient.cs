using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AwsDynamoDb
{
    public class DynamoDbClient
    {
        private IAmazonDynamoDB _dynamoDbClient;

        public DynamoDbClient(string serviceURL)
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

        public async Task<Dictionary<string,object>> GetItemAsync(string tableName, string id, IDynamoDbMapper mapper)
        {
            var getItemRq = new GetItemRequest 
            { 
                TableName = tableName, 
                Key = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { N = id } } }
            };
            var getItemTask = await _dynamoDbClient.GetItemAsync(getItemRq);
            var result = getItemTask.Item;

            if (getItemTask.Item.Count == 0)
                return null;

            return mapper.FromDynamoDb(result);
        }

        public async Task<List<Dictionary<string,object>>> ScanAsync(string tableName, IDynamoDbMapper mapper)
        {
            var scanRq = new ScanRequest { TableName = tableName };
            var scanTask = await _dynamoDbClient.ScanAsync(scanRq);
            var result = scanTask.Items;

            return result.Select(mapper.FromDynamoDb).ToList();
        }

        public async Task PutItemAsync(string tableName, string id, Dictionary<string, object> itemValue, IDynamoDbMapper mapper)
        {
            var dbItem = mapper.ToDynamoDb(itemValue);
            if (!dbItem.ContainsKey("Id"))
                dbItem.Add("Id", new AttributeValue{ N = id });
            var putItemTask = await _dynamoDbClient.PutItemAsync(tableName, dbItem);
        }

        public async Task DeleteItemAsync(string tableName, string itemId)
        {
            var dbItem = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { N = itemId } } }; 
            var deleteItemTask = await _dynamoDbClient.DeleteItemAsync(tableName, dbItem);
        }
    }
}