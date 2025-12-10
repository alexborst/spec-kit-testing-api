using SpecKitTestingApi.Interfaces;
using SpecKitTestingApi.Models;

namespace SpecKitTestingApi.Services;

/// <summary>
/// Provides weather forecast generation functionality.
/// Implements stateless design - no instance state maintained between calls.
/// </summary>
public sealed class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] WeatherSummaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Generates weather forecast data for the specified number of days.
    /// Method is pure and stateless - same input always produces deterministic output pattern.
    /// </summary>
    /// <param name="days">Number of days to forecast. Must be positive.</param>
    /// <returns>Collection of weather forecasts.</returns>
    /// <exception cref="ArgumentException">Thrown when days is not positive.</exception>
    public IEnumerable<WeatherForecast> GetForecast(int days = 5)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days, nameof(days));

        return Enumerable.Range(1, days)
            .Select(GenerateForecastForDay)
            .ToArray();
    }

    private static WeatherForecast GenerateForecastForDay(int dayOffset)
    {
        var forecastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(dayOffset));
        var temperature = Random.Shared.Next(-20, 55);
        var summary = WeatherSummaries[Random.Shared.Next(WeatherSummaries.Length)];

        return new WeatherForecast
        {
            Date = forecastDate,
            TemperatureInCelsius = temperature,
            Summary = summary
        };
    }
}