using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherVisualizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MinTempAndMaxSpeedStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE PROCEDURE GetMinTempAndMaxWindByCountry
                    @CountryName NVARCHAR(100),
                    @TemperatureThreshold FLOAT
                AS
                BEGIN
                    -- Select the minimum temperature and maximum wind speed for a given country 
                    -- where the minimum temperature is less than the specified threshold
                    SELECT 
                        MIN(TemperatureC) AS MinTemperature,
                        MAX(WindKph) AS MaxWindSpeed
                    FROM 
                        WeatherMeasurementEntries
                    WHERE 
                        Country = @CountryName
                        AND TemperatureC < @TemperatureThreshold;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP  PROCEDURE IF EXISTS dbo.GetMinTempAndMaxWindByCountry");
        }
    }
}
