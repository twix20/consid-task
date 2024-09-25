using System.Text.Json;
using WeatherVisualizer.Core.Entities;
using WeatherVisualizer.Infrastructure;

namespace WeatherVisualizer.Api;

public class WeatherHostedService : IHostedService, IDisposable
{
    private readonly ILogger<WeatherHostedService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer _timer;

    private readonly List<string> cities = new List<string>
    {
        "New York,US", "Los Angeles,US", "London,GB", "Paris,FR", "Berlin,DE",
        "Tokyo,JP", "Sydney,AU", "Toronto,CA", "Rio de Janeiro,BR", "Mumbai,IN"
    };

    private const string WeatherApiUrl = "http://api.weatherapi.com/v1/current.json";
    private const string ApiKey = "e06eecd33d294f66baa211024242509 "; // Replace with your RapidAPI key

    public WeatherHostedService(ILogger<WeatherHostedService> logger, IHttpClientFactory httpClientFactory, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        this._serviceScopeFactory = serviceScopeFactory;
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
                var response = await httpClient.GetAsync($"{WeatherApiUrl}?q={city}&key={ApiKey}");
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

    public class WeatherResponse
    {
        public Location Location { get; set; }
        public Current Current { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Tz_Id { get; set; }
        public long Localtime_Epoch { get; set; }
        public string Localtime { get; set; }
    }

    public class Current
    {
        public long Last_Updated_Epoch { get; set; }
        public string Last_Updated { get; set; }
        public double Temp_C { get; set; }
        public double Temp_F { get; set; }
        public int Is_Day { get; set; }
        public Condition Condition { get; set; }
        public double Wind_Mph { get; set; }
        public double Wind_Kph { get; set; }
        public int Wind_Degree { get; set; }
        public string Wind_Dir { get; set; }
        public double Pressure_Mb { get; set; }
        public double Pressure_In { get; set; }
        public double Precip_Mm { get; set; }
        public double Precip_In { get; set; }
        public int Humidity { get; set; }
        public int Cloud { get; set; }
        public double Feelslike_C { get; set; }
        public double Feelslike_F { get; set; }
        public double Windchill_C { get; set; }
        public double Windchill_F { get; set; }
        public double Heatindex_C { get; set; }
        public double Heatindex_F { get; set; }
        public double Dewpoint_C { get; set; }
        public double Dewpoint_F { get; set; }
        public double Vis_Km { get; set; }
        public double Vis_Miles { get; set; }
        public double Uv { get; set; }
        public double Gust_Mph { get; set; }
        public double Gust_Kph { get; set; }
    }

    public class Condition
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public int Code { get; set; }
    }
}
