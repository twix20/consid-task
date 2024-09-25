using Microsoft.EntityFrameworkCore;
using WeatherVisualizer.Core.Entities;

namespace WeatherVisualizer.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<WeatherMeasurementEntry> WeatherMeasurementEntries { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}
