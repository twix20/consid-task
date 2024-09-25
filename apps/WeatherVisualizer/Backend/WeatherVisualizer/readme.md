dotnet ef migrations add InitialCreate --project WeatherVisualizer.Infrastructure --startup-project WeatherVisualizer.Api --context AppDbContext --output-dir Migrations

dotnet ef database update --project WeatherVisualizer.Infrastructure --startup-project WeatherVisualizer.Api --context AppDbContext
