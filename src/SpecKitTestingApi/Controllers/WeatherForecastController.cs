using Microsoft.AspNetCore.Mvc;
using SpecKitTestingApi.Interfaces;
using SpecKitTestingApi.Models;

namespace SpecKitTestingApi.Controllers;

/// <summary>
/// Handles HTTP requests for weather forecast operations.
/// Controller is stateless and delegates business logic to services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _weatherForecastService;

    /// <summary>
    /// Initializes a new instance of the WeatherForecastController.
    /// </summary>
    /// <param name="weatherForecastService">Service for weather forecast operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when weatherForecastService is null.</exception>
    public WeatherForecastController(IWeatherForecastService weatherForecastService)
    {
        _weatherForecastService = weatherForecastService ?? throw new ArgumentNullException(nameof(weatherForecastService));
    }

    /// <summary>
    /// Retrieves weather forecast for the specified number of days.
    /// </summary>
    /// <param name="days">Number of days to forecast (default: 5, max: 30).</param>
    /// <returns>Collection of weather forecasts.</returns>
    /// <response code="200">Returns weather forecast data.</response>
    /// <response code="400">Invalid days parameter provided.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<WeatherForecast>> GetWeatherForecast([FromQuery] int days = 5)
    {
        if (days <= 0 || days > 30)
        {
            return BadRequest("Days parameter must be between 1 and 30.");
        }

        try
        {
            var forecast = _weatherForecastService.GetForecast(days);
            return Ok(forecast);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}