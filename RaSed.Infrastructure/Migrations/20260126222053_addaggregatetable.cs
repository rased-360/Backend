using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaSed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addaggregatetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedSensorData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvgTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MinTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AvgHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MinHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AvgPressure = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    MinPressure = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    MaxPressure = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    AvgHydrogen = table.Column<int>(type: "integer", nullable: false),
                    MinHydrogen = table.Column<int>(type: "integer", nullable: false),
                    MaxHydrogen = table.Column<int>(type: "integer", nullable: false),
                    AvgEthanol = table.Column<int>(type: "integer", nullable: false),
                    MinEthanol = table.Column<int>(type: "integer", nullable: false),
                    MaxEthanol = table.Column<int>(type: "integer", nullable: false),
                    ReadingCount = table.Column<int>(type: "integer", nullable: false),
                    AlertCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedSensorData", x => x.Id);
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedSensorData");
        }
    }
}
