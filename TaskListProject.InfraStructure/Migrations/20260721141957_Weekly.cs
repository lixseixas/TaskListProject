using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskListProject.Infrastructure.Migrations
{
    public partial class Weekly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyTaskReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalTasks = table.Column<int>(type: "int", nullable: false),
                    CompletedTasks = table.Column<int>(type: "int", nullable: false),
                    PendingTasks = table.Column<int>(type: "int", nullable: false),
                    CompletionPercentage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyTaskReports", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyTaskReports");
        }
    }
}
