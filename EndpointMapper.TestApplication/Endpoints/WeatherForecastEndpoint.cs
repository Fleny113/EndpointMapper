namespace EndpointMapper.TestApplication.Endpoints;

public abstract class WeatherForecastEndpoint : IEndpoint
{
    private static readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpMap(HttpMapMethod.Get, "/weatherForecast"), EndpointName("GetWeatherForecast")]
    public static WeatherForecast[] Handle()
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

        return forecast;
    }
}
