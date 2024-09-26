using System.Text.Json;
using WeatherVisualizer.Api.Models;
using WeatherVisualizer.Core.Entities;
using WeatherVisualizer.Infrastructure;

namespace WeatherVisualizer.Api;

public class WeatherHostedService : IHostedService, IDisposable
{
    private readonly ILogger<WeatherHostedService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConfiguration _configuration;
    private Timer _timer;

    private readonly List<string> cities = new List<string>
    {
        "New York,US", "Los Angeles,US", "London,GB", "Paris,FR", "Berlin,DE",
        "Tokyo,JP", "Sydney,AU", "Toronto,CA", "Rio de Janeiro,BR", "Mumbai,IN"
    };

    public WeatherHostedService(ILogger<WeatherHostedService> logger, IHttpClientFactory httpClientFactory, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WeatherHostedService is starting.");
        _timer = new Timer(FetchWeatherData, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
        return Task.CompletedTask;
    }

    private async void FetchWeatherData(object state)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        using var httpClient = _httpClientFactory.CreateClient();

        foreach (var city in cities)
        {
            try
            {
                var url = $"{_configuration["WeatherApi:BaseUrl"]}/v1/current.json?q={city}&key={_configuration["WeatherApi:ApiKey"]}";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var weatherMeasurementRecord = new WeatherMeasurementEntry
                {
                    City = weatherResponse.Location.Name,
                    Country = weatherResponse.Location.Country,
                    LastUpdated = DateTime.Parse(weatherResponse.Current.Last_Updated),
                    TemperatureC = weatherResponse.Current.Temp_C,
                    WindKph = weatherResponse.Current.Wind_Kph,
                };

                dbContext.WeatherMeasurementEntries.Add(weatherMeasurementRecord);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching weather data for {city}: {ex.Message}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WeatherHostedService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
