using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResultsService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerfThreadInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumberMessages = table.Column<int>(type: "int", nullable: false),
                    NumberConcurrentCalls = table.Column<int>(type: "int", nullable: false),
                    MinimumDuration = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Elapsed = table.Column<long>(type: "bigint", nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumCreated = table.Column<int>(type: "int", nullable: false),
                    RateCreation = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfThreadInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfThreadInfo");

            migrationBuilder.DropTable(
                name: "Runs");
        }
    }
}
