using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Amazon.DynamoDBv2.Model;
using Generator.Lambda;
using Generator.Domain;

namespace HelloWorld.Test
{
    public class DynamoDbRepositoryTests : IDisposable
    {
        private const string templatePath = "../../../../../src/Generator.Lambda/template.yaml"; // SAM template (CloudFormation)
        private const string tableLogicalName = "EntitiesTable"; // Logical name inside the template
        private const string dynamoDbLocalEndpoint = "http://localhost:8000"; // using local dynamodb
        private const string dynamoDbLocalTableName = "temp_entities"; 

        private LocalDynamoDbClient _localDynamoDbClient;
        private EntityDynamoDbRepository _entityRepository;

        public DynamoDbRepositoryTests()
        {
            _localDynamoDbClient = new LocalDynamoDbClient(dynamoDbLocalEndpoint);
            _localDynamoDbClient.CreateTable(dynamoDbLocalTableName, templatePath, tableLogicalName);
            
            _entityRepository = new EntityDynamoDbRepository(dynamoDbLocalTableName, dynamoDbLocalEndpoint);
        }

        [Fact]
        public void TestCreateLocalTable()
        {
            var tableName = "generator_entities";
            if (this._localDynamoDbClient.TableExists(tableName))
            {
                Console.WriteLine($"Table {tableName} exists, deleting it.");
                this._localDynamoDbClient.DeleteTable(tableName);
                Console.WriteLine($"Table {tableName} deleted.");
            }

            Console.WriteLine($"Creating DynamoDB table {tableName}");
            this._localDynamoDbClient.CreateTable(tableName, templatePath, tableLogicalName);
            Console.WriteLine($"DynamoDB table {tableName} created");
        }

        [Fact]
        public async Task TestPutSomeDataInDynamoDbAsync()
        {
            Console.WriteLine("Generating items ...");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var entity = new Entity
                    {
                        UserId = i.ToString(),
                        Name = $"entity{i}-{j}"
                    };
                    await _entityRepository.PutItemAsync(entity);
                    Console.WriteLine($"Created item {i}-{j}");
                }
            }
            Console.WriteLine("Done");

            Console.WriteLine("Listing data for user 0");
            var list0 = await _entityRepository.GetEntitiesByUserAsync("0");
            foreach (var item in list0)
            {
                Console.WriteLine($"user id = {item.UserId}, entity id = {item.Id}, entity name = {item.Name}");
            }

            Console.WriteLine("Listing data for user 1");
            var list1 = await _entityRepository.GetEntitiesByUserAsync("1");
            foreach (var item in list1)
            {
                Console.WriteLine($"user id = {item.UserId}, entity id = {item.Id}, entity name = {item.Name}");
            }
        }

        public void Dispose()
        {
            this._localDynamoDbClient.DeleteTable(dynamoDbLocalTableName);
            Console.WriteLine($"DynamoDB table {dynamoDbLocalTableName} deleted");
        }
    }
}
