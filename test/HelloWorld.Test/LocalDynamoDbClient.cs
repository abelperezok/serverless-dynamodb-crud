using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading;

namespace HelloWorld.Tests
{
    public class LocalDynamoDbClient
    {
        private readonly IAmazonDynamoDB dynamoDbClient;
        private readonly Action<string> logAction;

        public LocalDynamoDbClient(string localendpoint, Action<string> log = null)
        {
            this.dynamoDbClient = CreateDynamoDBClient(localendpoint);
            this.logAction = log != null ? log : (m) => { Console.WriteLine(m); };
        }

        private IAmazonDynamoDB CreateDynamoDBClient(string localEndpoint)
        {
            var config = new AmazonDynamoDBConfig 
            {
                ServiceURL = localEndpoint
            };
            return new AmazonDynamoDBClient(config);
        }

        private async Task<CreateTableResponse> CreateTableAsync(string tableName)
        {
            var createTableReq = new CreateTableRequest
            {
                TableName = tableName,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement {AttributeName = "user_id", KeyType = KeyType.HASH },
                    new KeySchemaElement {AttributeName = "entity_id", KeyType = KeyType.RANGE }
                },
                AttributeDefinitions = new List<AttributeDefinition>{
                    new AttributeDefinition { AttributeName = "user_id", AttributeType = ScalarAttributeType.S },
                    new AttributeDefinition { AttributeName = "entity_id", AttributeType = ScalarAttributeType.S },
                    new AttributeDefinition { AttributeName = "entity_name", AttributeType = ScalarAttributeType.S }
                },
                ProvisionedThroughput = new ProvisionedThroughput(5, 5),
                LocalSecondaryIndexes = new List<LocalSecondaryIndex>
                {
                    new LocalSecondaryIndex
                    {
                        IndexName = "entity_id_name",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement {AttributeName = "user_id", KeyType = KeyType.HASH },
                            new KeySchemaElement {AttributeName = "entity_name", KeyType = KeyType.RANGE }
                        },
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.KEYS_ONLY
                        }
                    }
                }
            };

            return await dynamoDbClient.CreateTableAsync(createTableReq);
        }

        private void WaitUntilTableIsActive(string tableName)
        {
            var currentStatus = TableStatus.CREATING;
            do 
            {
                logAction($"Checking if the Table is ready ... Currently is {currentStatus}");
                var describeTable = dynamoDbClient.DescribeTableAsync(tableName);
                describeTable.Wait();
                currentStatus = describeTable.Result.Table.TableStatus;
                Thread.Sleep(3000);
            } 
            while (currentStatus != TableStatus.ACTIVE);
            logAction("Table ready !");
        }

        private async Task<DeleteTableResponse> DeleteTableAsync(string tableName)
        {
            return await dynamoDbClient.DeleteTableAsync(tableName);
        }

        public void CreateTable(string tableName)
        {
            var table = CreateTableAsync(tableName);
            table.Wait();
            WaitUntilTableIsActive(tableName);
        }

        public void DeleteTable(string tableName)
        {
            var delete = DeleteTableAsync(tableName);
            delete.Wait();
        }
    }
}
