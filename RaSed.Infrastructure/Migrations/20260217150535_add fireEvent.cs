using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaSed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addfireEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedSensorData");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Issues",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "FireEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FireEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FireEvents_DeviceId_Status",
                table: "FireEvents",
                columns: new[] { "DeviceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FireEvents_StartTime",
                table: "FireEvents",
                column: "StartTime",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FireEvents");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Issues",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AggregatedSensorData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertCount = table.Column<int>(type: "integer", nullable: false),
                    AvgEthanol = table.Column<int>(type: "integer", nullable: false),
                    AvgHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AvgHydrogen = table.Column<int>(type: "integer", nullable: false),
                    AvgPressure = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    AvgTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxEthanol = table.Column<int>(type: "integer", nullable: false),
                    MaxHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxHydrogen = table.Column<int>(type: "integer", nullable: false),
                    MaxPressure = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    MaxTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MinEthanol = table.Column<int>(type: "integer", nullable: false),
                    MinHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MinHydrogen = table.Column<int>(type: "integer", nullable: false),
                    MinPressure = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    MinTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ReadingCount = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedSensorData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AlertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ethanol = table.Column<int>(type: "integer", nullable: false),
                    HasAlert = table.Column<bool>(type: "boolean", nullable: false),
                    HeatIndex = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    Humidity = table.Column<decimal>(type: "numeric", nullable: false),
                    Hydrogen = table.Column<int>(type: "integer", nullable: false),
                    Pressure = table.Column<decimal>(type: "numeric", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedSensorData_EndTime",
                table: "AggregatedSensorData",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedSensorData_StartTime",
                table: "AggregatedSensorData",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedSensorData_TimeRange",
                table: "AggregatedSensorData",
                columns: new[] { "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_Timestamp",
                table: "SensorReadings",
                column: "Timestamp");
        }
    }
}
