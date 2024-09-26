using Microsoft.EntityFrameworkCore;
using WeatherVisualizer.Api;
using WeatherVisualizer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddHostedService<WeatherHostedService>();


var app = builder.Build();

app.UseCors((builder) =>
{
    builder.AllowAnyMethod().AllowAnyOrigin().AllowAnyMethod();
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
