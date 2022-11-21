using Microsoft.AspNetCore.Mvc;
using kundeAPI.Models;
using kundeAPI.Controllers;
using System.Net;
using System.Diagnostics;

namespace ServiceA.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private IConfiguration _config;
    private string hostname;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        hostname = System.Net.Dns.GetHostName();
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("GetServiceA")]
    [ProducesResponseType(typeof((string hostname, int time)), 200)]
    public IActionResult GetServiceA()
    {
        var isFail = _config["ToFail"] == "yes";
        _logger.LogDebug($"{hostname} is set to {isFail}");

        var seconds = DateTime.Now.Second;
        var hasError = (seconds > 30 && seconds < 45);
        if (hasError && isFail) {
            return StatusCode(503);
        } else {
            return Ok( new { hostname, seconds } );
        }
    }
}
