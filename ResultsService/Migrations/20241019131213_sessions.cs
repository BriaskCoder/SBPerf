using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResultsService.Migrations
{
    /// <inheritdoc />
    public partial class sessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Sessions",
                table: "PerfThreadInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sessions",
                table: "PerfThreadInfo");
        }
    }
}
