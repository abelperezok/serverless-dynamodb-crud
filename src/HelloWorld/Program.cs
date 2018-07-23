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
        private string Json(object item)
        {
            var json = item != null 
                ? JsonConvert.SerializeObject(item, Formatting.Indented)
                : " ";
            return json + Environment.NewLine;
        }

        private APIGatewayProxyResponse JsonAPIGatewayProxyResponse(object body = null, int statusCode = 200)
        {
            return new APIGatewayProxyResponse
            {
                Body = Json(body),
                StatusCode = statusCode,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private static readonly Lazy<EntityDynamoDbRepository> _dynamoDbRepo = new Lazy<EntityDynamoDbRepository>(
            () => {
                var samLocal = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL");
                if (samLocal != null)
                    return new EntityDynamoDbRepository("http://dynamodb:8000"); //using localstack
                return new EntityDynamoDbRepository(null);
             }
        );
        private static EntityDynamoDbRepository entitiesRepo => _dynamoDbRepo.Value;







        public async Task<APIGatewayProxyResponse> GetListFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var userid = apigProxyEvent.PathParameters["userid"];
            var list = await entitiesRepo.QueryEntitiesByUserAsync(table, userid);
            return JsonAPIGatewayProxyResponse(list);
        }

        public async Task<APIGatewayProxyResponse> GetItemFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var userid = apigProxyEvent.PathParameters["userid"];
            var entityid = apigProxyEvent.PathParameters["entityid"];

            var item = await entitiesRepo.GetItemAsync(table, userid, entityid);

            if (item == null)
               return JsonAPIGatewayProxyResponse(null, 404);

            return JsonAPIGatewayProxyResponse(item);
        }

        public async Task<APIGatewayProxyResponse> PostFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var entity = JsonConvert.DeserializeObject<Entity>(apigProxyEvent.Body);
            entity.UserId = apigProxyEvent.PathParameters["userid"];

            var result = await entitiesRepo.PutItemAsync(table, entity);

            return JsonAPIGatewayProxyResponse(result);
        }

        public async Task<APIGatewayProxyResponse> PutFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var entity = JsonConvert.DeserializeObject<Entity>(apigProxyEvent.Body);
            entity.Id = apigProxyEvent.PathParameters["entityid"];
            entity.UserId = apigProxyEvent.PathParameters["userid"];

            var result = await entitiesRepo.PutItemAsync(table, entity);

            return JsonAPIGatewayProxyResponse(result);
        }

        public async Task<APIGatewayProxyResponse> DeleteFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var entity = new Entity();
            entity.Id = apigProxyEvent.PathParameters["entityid"];
            entity.UserId = apigProxyEvent.PathParameters["userid"];

            await entitiesRepo.DeleteItemAsync(table, entity);

            return JsonAPIGatewayProxyResponse();
        }
    }
}
