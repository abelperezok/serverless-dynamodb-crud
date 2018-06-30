using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using AwsDynamoDb;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HelloWorld
{

    public class Function
    {
        // private static readonly Lazy<DynamoDbClient> _dynamoDb = new Lazy<DynamoDbClient>(
        //     () => {
        //         var samLocal = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL");
        //         if (samLocal != null)
        //             return new DynamoDbClient("http://localstack:4569");
        //         return new DynamoDbClient(null);
        //      }
        // );

        // private static DynamoDbClient dynamoDb => _dynamoDb.Value;
        // private static IDynamoDbMapper entityMapper = new EntityDynamoDbMapper();









        private static readonly Lazy<EntityDynamoDbRepository> _dynamoDbRepo = new Lazy<EntityDynamoDbRepository>(
            () => {
                var samLocal = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL");
                if (samLocal != null)
                    return new EntityDynamoDbRepository("http://localstack:4569");
                return new EntityDynamoDbRepository(null);
             }
        );
        private static EntityDynamoDbRepository dynamoDbRepo => _dynamoDbRepo.Value;


        public async Task<APIGatewayProxyResponse> GetFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");

            var list = await dynamoDbRepo.ScanAsync(table);

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(list),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public async Task<APIGatewayProxyResponse> PostFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var body = JsonConvert.DeserializeObject<Dictionary<string,object>>(apigProxyEvent.Body);
            var id = Convert.ToString(body["Id"]);

            //await dynamoDb.PutItemAsync(table, id, body, entityMapper);

            return new APIGatewayProxyResponse
            {
                Body = "OK",
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }




        // public async Task<APIGatewayProxyResponse> GetFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        // {
        //     var table = Environment.GetEnvironmentVariable("TABLE_NAME");
        //     var list = await dynamoDb.ScanAsync(table, entityMapper);

        //     return new APIGatewayProxyResponse
        //     {
        //         Body = JsonConvert.SerializeObject(list),
        //         StatusCode = 200,
        //         Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //     };
        // }

        // public async Task<APIGatewayProxyResponse> PostFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        // {
        //     var table = Environment.GetEnvironmentVariable("TABLE_NAME");
        //     var body = JsonConvert.DeserializeObject<Dictionary<string,object>>(apigProxyEvent.Body);
        //     var id = Convert.ToString(body["Id"]);

        //     await dynamoDb.PutItemAsync(table, id, body, entityMapper);

        //     return new APIGatewayProxyResponse
        //     {
        //         Body = "OK",
        //         StatusCode = 200,
        //         Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //     };
        // }


        // public async Task<APIGatewayProxyResponse> PutFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        // {
        //     var table = Environment.GetEnvironmentVariable("TABLE_NAME");
        //     var id = apigProxyEvent.PathParameters["id"];
        //     var body = JsonConvert.DeserializeObject<Dictionary<string,object>>(apigProxyEvent.Body);

        //     var item = await dynamoDb.GetItemAsync(table, id, entityMapper);
        //     if (item == null)
        //         return new APIGatewayProxyResponse 
        //         {
        //             Body = "Not Found",
        //             StatusCode = 404
        //         };

        //     await dynamoDb.PutItemAsync(table, id, body, entityMapper);

        //     return new APIGatewayProxyResponse
        //     {
        //         Body = @"{""result"":""success""}",
        //         StatusCode = 200,
        //         Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //     };
        // }

        // public async Task<APIGatewayProxyResponse> DeleteFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        // {
        //     var table = Environment.GetEnvironmentVariable("TABLE_NAME");
        //     var id = apigProxyEvent.PathParameters["id"];

        //     var item = await dynamoDb.GetItemAsync(table, id, entityMapper);
        //     if (item == null)
        //         return new APIGatewayProxyResponse 
        //         {
        //             Body = "Not Found",
        //             StatusCode = 404
        //         };

        //     await dynamoDb.DeleteItemAsync(table, id);

        //     return new APIGatewayProxyResponse
        //     {
        //         Body = @"{""result"":""success""}",
        //         StatusCode = 200,
        //         Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //     };
        // }
    }
}
