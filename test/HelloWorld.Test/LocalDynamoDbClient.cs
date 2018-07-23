using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading;

namespace HelloWorld.Test
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

        public bool TableExists(string tableName)
        {
            var tableTask = dynamoDbClient.ListTablesAsync();
            tableTask.Wait();
            return tableTask.Result.TableNames.Contains(tableName);
        }

        public void CreateTable(string tableName, string templatePath, string logicalName)
        {
            var createTableReq = TemplateParser.GetDynamoDbTable(templatePath, logicalName);
            createTableReq.TableName = tableName;
            var tableTask = dynamoDbClient.CreateTableAsync(createTableReq);
            tableTask.Wait();
            WaitUntilTableIsActive(tableName);
        }

        public void DeleteTable(string tableName)
        {
            var deleteTask = dynamoDbClient.DeleteTableAsync(tableName);
            deleteTask.Wait();
        }
    }
}
