using Microsoft.AspNetCore.Authorization;

namespace Beseler.ApiService.Application;

internal static class WeatherEndpoints
{
    private static readonly string[] _summaries =
        [
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        ];

    public static WebApplication MapWeatherEndpoints(this WebApplication app)
    {
        app.MapGet("/weather", [AllowAnonymous] () =>
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = _summaries[Random.Shared.Next(_summaries.Length)]
            });

            return TypedResults.Ok(forecasts);
        }).RequireAuthorization();

        return app;
    }
}
