using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace Generator.Lambda
{
    public class APIGatewayProxyHelper
    {
        private static string Json(object item)
        {
            var json = item != null
                ? JsonConvert.SerializeObject(item, Formatting.Indented)
                : null;
            return json + Environment.NewLine;
        }

        public static APIGatewayProxyResponse JsonAPIGatewayProxyResponse(object body = null, int statusCode = 200)
        {
            return new APIGatewayProxyResponse
            {
                Body = Json(body),
                StatusCode = statusCode,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
