using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Generator.Domain;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Generator.Lambda
{
    public class RepositoryFactory
    {
        public static IEntityRepository CreateEntityRepository(string tableName)
        {
            var samLocal = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL");
            if (samLocal != null)
                return new EntityDynamoDbRepository(tableName, "http://dynamodb:8000"); //using localstack
            return new EntityDynamoDbRepository(tableName);
        }
    }

    public class EntitiesFunctions
    {
        public async Task<APIGatewayProxyResponse> GetListFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var userid = apigProxyEvent.PathParameters["userid"];
            var entitiesRepo = RepositoryFactory.CreateEntityRepository(table);
            var list = await entitiesRepo.GetEntitiesByUserAsync(userid);
            return APIGatewayProxyHelper.JsonAPIGatewayProxyResponse(list);
        }

        public async Task<APIGatewayProxyResponse> GetItemFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var userid = apigProxyEvent.PathParameters["userid"];
            var entityid = apigProxyEvent.PathParameters["entityid"];

            var entitiesRepo = RepositoryFactory.CreateEntityRepository(table);
            var item = await entitiesRepo.GetItemAsync(userid, entityid);

            if (item == null)
                return APIGatewayProxyHelper.JsonAPIGatewayProxyResponse(null, 404);

            return APIGatewayProxyHelper.JsonAPIGatewayProxyResponse(item);
        }

        public async Task<APIGatewayProxyResponse> PostFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var entity = JsonConvert.DeserializeObject<Entity>(apigProxyEvent.Body);
            entity.UserId = apigProxyEvent.PathParameters["userid"];

            var entitiesRepo = RepositoryFactory.CreateEntityRepository(table);
            var result = await entitiesRepo.PutItemAsync(entity);

            return APIGatewayProxyHelper.JsonAPIGatewayProxyResponse(result);
        }

        public async Task<APIGatewayProxyResponse> PutFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var entity = JsonConvert.DeserializeObject<Entity>(apigProxyEvent.Body);
            entity.Id = apigProxyEvent.PathParameters["entityid"];
            entity.UserId = apigProxyEvent.PathParameters["userid"];

            var entitiesRepo = RepositoryFactory.CreateEntityRepository(table);
            var result = await entitiesRepo.PutItemAsync(entity);

            return APIGatewayProxyHelper.JsonAPIGatewayProxyResponse(result);
        }

        public async Task<APIGatewayProxyResponse> DeleteFunctionHandlerAsync(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var table = Environment.GetEnvironmentVariable("TABLE_NAME");
            var entity = new Entity();
            entity.Id = apigProxyEvent.PathParameters["entityid"];
            entity.UserId = apigProxyEvent.PathParameters["userid"];

            var entitiesRepo = RepositoryFactory.CreateEntityRepository(table);
            await entitiesRepo.DeleteItemAsync(entity);

            return APIGatewayProxyHelper.JsonAPIGatewayProxyResponse();
        }
    }
}
