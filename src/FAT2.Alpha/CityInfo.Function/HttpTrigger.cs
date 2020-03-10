using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CityInfo.Function
{
    public static class HttpTrigger
    {
        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{cityName}")] HttpRequest req,
            string cityName,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request, city name: [{cityName}].");

            return cityName != null
                ? (ActionResult)new OkObjectResult($"Hello, {cityName}")
                : new BadRequestObjectResult("Please pass a city name in the route");
        }
    }
}
