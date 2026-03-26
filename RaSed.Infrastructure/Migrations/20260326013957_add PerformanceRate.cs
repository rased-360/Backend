using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaSed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPerformanceRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PerformanceLastUpdatedAt",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PerformanceRate",
                table: "Employees",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 100.0);

            migrationBuilder.AddColumn<string>(
                name: "PerformanceRating",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Excellent");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PerformanceRate",
                table: "Employees",
                column: "PerformanceRate",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_PerformanceRate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PerformanceLastUpdatedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PerformanceRate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PerformanceRating",
                table: "Employees");
        }
    }
}
