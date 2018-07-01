using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Amazon.DynamoDBv2.Model;

namespace HelloWorld.Tests
{
    public class DynamoDbRepositoryTests : IDisposable
    {
        private static string dynamoDbLocalEndpoint = "http://localhost:4569"; // using localstack
        private static string dynamoDbLocalTableName = "myproject_entities"; 

        private LocalDynamoDbClient localDynamoDbClient;
        private EntityDynamoDbRepository entityRepository;

        public DynamoDbRepositoryTests()
        {
            this.localDynamoDbClient = new LocalDynamoDbClient(dynamoDbLocalEndpoint);
            this.localDynamoDbClient.CreateTable(dynamoDbLocalTableName);
            this.entityRepository = new EntityDynamoDbRepository(dynamoDbLocalEndpoint);
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
                    await entityRepository.PutItemAsync(dynamoDbLocalTableName, entity);
                    Console.WriteLine($"Created item {i}-{j}");
                }
            }
            Console.WriteLine("Done");

            Console.WriteLine("Listing data for user 0");
            var list0 = await entityRepository.QueryEntitiesByUserAsync(dynamoDbLocalTableName, "0");
            foreach (var item in list0)
            {
                Console.WriteLine($"user id = {item.UserId}, entity id = {item.Id}, entity name = {item.Name}");
            }

            Console.WriteLine("Listing data for user 1");
            var list1 = await entityRepository.QueryEntitiesByUserAsync(dynamoDbLocalTableName, "1");
            foreach (var item in list1)
            {
                Console.WriteLine($"user id = {item.UserId}, entity id = {item.Id}, entity name = {item.Name}");
            }
        }

        public void Dispose()
        {
            this.localDynamoDbClient.DeleteTable(dynamoDbLocalTableName);
            Console.WriteLine("DynamoDB table deleted");
        }
    }
}
