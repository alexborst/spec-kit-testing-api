namespace SpecKitTestingApi.Models;

/// <summary>
/// Represents weather forecast data for a specific date.
/// </summary>
public sealed record WeatherForecast
{
    public DateOnly Date { get; init; }
    public int TemperatureInCelsius { get; init; }
    public string? Summary { get; init; }
    
    /// <summary>
    /// Calculates temperature in Fahrenheit from Celsius.
    /// </summary>
    public int TemperatureInFahrenheit => 32 + (int)(TemperatureInCelsius / 0.5556);
}