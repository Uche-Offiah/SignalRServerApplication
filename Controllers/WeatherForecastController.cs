using Microsoft.AspNetCore.Mvc;
using SignalRChatApplication.Models;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SignalRChatApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly HttpClient _client;
        private readonly IDatabase _redis;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, HttpClient client, IConnectionMultiplexer muxer)
        {
            _logger = logger;
            _client = client;
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weatherCachingApp", "1.0"));
            _redis = muxer.GetDatabase();
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecastMain> Get()
        //{
        //    try
        //    {
        //        return Enumerable.Range(1, 5).Select(index => new WeatherForecastMain
        //        {
        //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //            TemperatureC = Random.Shared.Next(-20, 55),
        //            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //        }).ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        throw;
        //    }
        //}

        [HttpGet(Name = "TestWeatherForecast")]
        public string GetTest()
        {
            _logger.LogError("I just encountered an error");
            return "me";
        }

        private string GetForecast(double latitude, double longitude)
        {
            //var pointsRequestQuery = $"https://api.weather.gov/points/{latitude},{longitude}"; //get the URI
            //var result = await _client.GetFromJsonAsync<JsonObject>(pointsRequestQuery);

            //var gridX = result["properties"]["gridX"].ToString();
            //var gridY = result["properties"]["gridY"].ToString();
            //var gridId = result["Properties"]["gridId"].ToString();
            //var forecastRequestQuery = $"https://api.weather.gov/gridpoints/{gridId}/{gridX},{gridY}/forecast";
            //var forecastResult = await _client.GetFromJsonAsync<JsonObject>(forecastRequestQuery);
            //var periodsJson = forecastResult["properties"]["periods"].ToJsonString();
            var forecastResult = new List<WeatherForecast>();
            
            var item = new WeatherForecast
            {
                Name = "Summer",
                Number = 2,
                WindSpeed = "25KM/H",
                WindDirection = "East"
            };
            forecastResult.Add(item);
            return JsonSerializer.Serialize(forecastResult);
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ForecastResult> Get([FromQuery] double latitude, [FromQuery] double longitude)
        {
            string json;
            var watch = Stopwatch.StartNew();
            var keyName = $"forecast:{latitude},{longitude}";
            json = await _redis.StringGetAsync(keyName);
            if (string.IsNullOrEmpty(json))
            {
                json =  GetForecast(latitude, longitude);
                var setTask = _redis.StringSetAsync(keyName, json);
                var expireTask = _redis.KeyExpireAsync(keyName, TimeSpan.FromSeconds(3600));
                await Task.WhenAll(setTask, expireTask);
            }

            var forecast =  JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(json);
            watch.Stop();
            var result = new ForecastResult(forecast, watch.ElapsedMilliseconds);
            Utility.LogStuff(json, "RedisFolder");

            return result;
        }
    }
}
