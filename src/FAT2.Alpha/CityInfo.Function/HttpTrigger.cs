using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CityInfo.Function
{
    public static class HttpTrigger
    {
        public const double AbsoluteZeroCelsius = -273.15;

        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{cityName}")] HttpRequest req,
            string cityName,
            ILogger log)
        {
            log.LogInformation($"HttpTrigger function received a request, city name: [{cityName}].");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("local.settings.json", optional:true, reloadOnChange:true)
                .AddEnvironmentVariables()
                .Build();

            var weatherAppId = config["weather-app-id"];
            var weatherUri = $"http://api.openweathermap.org/data/2.5/forecast?q={cityName},uk&APPID={weatherAppId}";
            var populationUri = $"https://public.opendatasoft.com/api/records/1.0/search/?dataset=worldcitiespop&q={cityName}+&facet=country&refine.country=gb";

            string weatherResult;
            string populationResult;

            using (var client = new HttpClient())
            {
                var weatherResponse = await client.GetAsync(weatherUri).ConfigureAwait(false);
                weatherResponse.EnsureSuccessStatusCode();
                weatherResult = await weatherResponse.Content
                    .ReadAsStringAsync().ConfigureAwait(false);

                var populationResponse = await client.GetAsync(populationUri).ConfigureAwait(false);
                populationResponse.EnsureSuccessStatusCode();
                populationResult = await populationResponse.Content
                    .ReadAsStringAsync().ConfigureAwait(false);
            }

            var weatherJson = JObject.Parse(weatherResult);
            var populationJson = JObject.Parse(populationResult);

            var response = new CityInfoResponse
            {
                City = populationJson.SelectToken("records[0].fields.city").Value<string>(),
                MaxTemp = weatherJson.SelectToken("list[0].main.temp_max").Value<double>() + AbsoluteZeroCelsius,
                MinTemp = weatherJson.SelectToken("list[0].main.temp_min").Value<double>() + AbsoluteZeroCelsius,
                Population = populationJson.SelectToken("records[0].fields.population").Value<int>()
            };

            log.LogInformation($"HttpTrigger function processed a request, city name: [{cityName}].");

            return cityName != null
                ? (ActionResult)new OkObjectResult(response)
                : new BadRequestObjectResult("Please pass a city name in the route");
        }
    }

    public class CityInfoResponse
    {
        public string City { get; set; }
        public double? MaxTemp { get; set; }
        public double? MinTemp { get; set; }
        public int? Population { get; set; }
    }
}
