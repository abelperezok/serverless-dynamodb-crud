using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.Text;

namespace HelloWorld.Tests
{
    public class ApiIntegrationTests
    {
        private static readonly HttpClient client = new HttpClient();
        private static string baseUrl = "http://127.0.0.1:3000";

        [Fact]
        public async Task TestGetFunctionHandlerAsync()
        {
            Console.WriteLine("Make sure you started 'sam local start-api'");

            var response = await client.GetAsync(baseUrl + "/entities");
            
            Assert.Equal(200, (int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("[", content);
            Assert.EndsWith("]", content);
        }

        [Fact]
        public async Task TestPostFunctionHandlerAsync()
        {
            Console.WriteLine("Make sure you started 'sam local start-api'");

            var entity = new { Id = 1, Name = "NewEntity" };
            var entityJson = JsonConvert.SerializeObject(entity);
            var stringContent = new StringContent(entityJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl + "/entities", stringContent);
            
            Assert.Equal(200, (int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("OK", content);
        }

        [Fact]
        public async Task TestPutFunctionHandlerAsync()
        {
            Console.WriteLine("Make sure you started 'sam local start-api'");

            var entity = new { Name = "NewEntity-Updated" };
            var entityJson = JsonConvert.SerializeObject(entity);
            var stringContent = new StringContent(entityJson, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(baseUrl + "/entities/1", stringContent);
            
            Assert.Equal(200, (int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"result\":\"success\"}", content);
        }

        [Fact]
        public async Task TestDeleteFunctionHandlerAsync()
        {
            Console.WriteLine("Make sure you started 'sam local start-api'");

            var response = await client.DeleteAsync(baseUrl + "/entities/1");
            
            Assert.Equal(200, (int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"result\":\"success\"}", content);
        }
    }
}
