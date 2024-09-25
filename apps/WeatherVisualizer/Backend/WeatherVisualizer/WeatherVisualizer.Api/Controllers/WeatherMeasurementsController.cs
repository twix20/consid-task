using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherVisualizer.Core.Entities;
using WeatherVisualizer.Infrastructure;

namespace WeatherVisualizer.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherMeasurementsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public WeatherMeasurementsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherMeasurementEntry>> Get()
    {
        return await _dbContext.WeatherMeasurementEntries.ToListAsync();
    }
}
