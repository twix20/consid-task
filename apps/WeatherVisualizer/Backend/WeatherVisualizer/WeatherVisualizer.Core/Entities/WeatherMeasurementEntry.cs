namespace WeatherVisualizer.Core.Entities;

public class WeatherMeasurementEntry
{
    public Guid Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public double TemperatureC { get; set; }
    public double WindKph { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}