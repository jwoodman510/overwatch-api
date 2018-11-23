using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace checkup
{
    public class Function
    {
        private const string Endpoint = "https://overwatch-dashboard.azurewebsites.net/api/health/stats?deep=true";

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(Endpoint);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                return "unavailable";
            }
        }
    }
}
