using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherVisualizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MaxWindSpeedStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE PROCEDURE GetMaxWindSpeedByCountry
                    @CountryName NVARCHAR(100)
                AS
                BEGIN
                    -- Select the maximum wind speed and the country
                    SELECT TOP 1
                        MAX(WindKph) AS MaxWindSpeed,
                        Country
                    FROM
                        WeatherMeasurementEntries
                    WHERE
                        Country = @CountryName
                    GROUP BY
                        Country;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP  PROCEDURE IF EXISTS dbo.GetMaxWindSpeedByCountry");
        }
    }
}
