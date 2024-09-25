using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherVisualizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameWeatherEntryProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "WeatherMeasurementEntries");

            migrationBuilder.DropColumn(
                name: "WindSpeed",
                table: "WeatherMeasurementEntries");

            migrationBuilder.RenameColumn(
                name: "MeasurementTime",
                table: "WeatherMeasurementEntries",
                newName: "LastUpdated");

            migrationBuilder.AddColumn<double>(
                name: "TemperatureC",
                table: "WeatherMeasurementEntries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WindKph",
                table: "WeatherMeasurementEntries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemperatureC",
                table: "WeatherMeasurementEntries");

            migrationBuilder.DropColumn(
                name: "WindKph",
                table: "WeatherMeasurementEntries");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "WeatherMeasurementEntries",
                newName: "MeasurementTime");

            migrationBuilder.AddColumn<float>(
                name: "Temperature",
                table: "WeatherMeasurementEntries",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "WindSpeed",
                table: "WeatherMeasurementEntries",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
