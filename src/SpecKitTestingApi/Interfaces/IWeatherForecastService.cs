using SpecKitTestingApi.Models;

namespace SpecKitTestingApi.Interfaces;

/// <summary>
/// Defines contract for weather forecast operations.
/// </summary>
public interface IWeatherForecastService
{
    /// <summary>
    /// Generates weather forecast data for the next specified number of days.
    /// </summary>
    /// <param name="days">Number of days to forecast.</param>
    /// <returns>Collection of weather forecasts.</returns>
    IEnumerable<WeatherForecast> GetForecast(int days = 5);
}