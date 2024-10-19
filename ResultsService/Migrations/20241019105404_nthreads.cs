﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResultsService.Migrations
{
    /// <inheritdoc />
    public partial class nthreads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberThreads",
                table: "PerfThreadInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberThreads",
                table: "PerfThreadInfo");
        }
    }
}
