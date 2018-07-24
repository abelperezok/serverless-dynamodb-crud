using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;
using Xunit;
using System.Text;
using Generator.Domain;

namespace HelloWorld.Test
{
    public class ApiIntegrationTests
    {
        private static readonly HttpClient client = new HttpClient();
        private static string baseUrl = "http://127.0.0.1:3000";

        [Fact]
        public async Task TestCrudSequence()
        {
            Console.WriteLine("Make sure you started 'sam local start-api'");

            Console.WriteLine("Testing empty GET");
            var getResponse = await client.GetAsync(baseUrl + "/users/localuser/entities");
            Assert.Equal(200, (int)getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getList = JsonConvert.DeserializeObject<List<Entity>>(getContent);
            Assert.Equal(0, getList.Count);

            Console.WriteLine("Testing basic POST");
            var postEntity = new { Name = "NewEntity" };
            var postEntityJson = JsonConvert.SerializeObject(postEntity);
            var postStringContent = new StringContent(postEntityJson, Encoding.UTF8, "application/json");
            var postResponse = await client.PostAsync(baseUrl + "/users/localuser/entities", postStringContent);
            Assert.Equal(200, (int)postResponse.StatusCode);
            var postContent = await postResponse.Content.ReadAsStringAsync();
            var postResult = JsonConvert.DeserializeObject<Entity>(postContent);
            Assert.Equal("NewEntity", postResult.Name);

            Console.WriteLine("Testing basic PUT");
            var putEntity = new { Name = "NewEntity-Updated" };
            var putEntityJson = JsonConvert.SerializeObject(putEntity);
            var putSringContent = new StringContent(putEntityJson, Encoding.UTF8, "application/json");
            var putResponse = await client.PutAsync(baseUrl + "/users/localuser/entities/" + postResult.Id, putSringContent);
            Assert.Equal(200, (int)putResponse.StatusCode);
            var putContent = await putResponse.Content.ReadAsStringAsync();
            var putResult = JsonConvert.DeserializeObject<Entity>(putContent);
            Assert.Equal("NewEntity-Updated", putResult.Name);


            Console.WriteLine("Testing basic DELETE");
            var deleteResponse = await client.DeleteAsync(baseUrl + "/users/localuser/entities/"+ postResult.Id);
            Assert.Equal(200, (int)deleteResponse.StatusCode);
            var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
            Assert.True(string.IsNullOrWhiteSpace(deleteContent));
        }
    }
}
