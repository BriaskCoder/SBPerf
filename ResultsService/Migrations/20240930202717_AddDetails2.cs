using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResultsService.Migrations
{
    /// <inheritdoc />
    public partial class AddDetails2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RateCreation",
                table: "PerfThreadInfo",
                newName: "ActualRate");

            migrationBuilder.AlterColumn<decimal>(
                name: "Elapsed",
                table: "PerfThreadInfo",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "ActualNumberMessages",
                table: "PerfThreadInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualNumberMessages",
                table: "PerfThreadInfo");

            migrationBuilder.RenameColumn(
                name: "ActualRate",
                table: "PerfThreadInfo",
                newName: "RateCreation");

            migrationBuilder.AlterColumn<long>(
                name: "Elapsed",
                table: "PerfThreadInfo",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
