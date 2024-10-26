using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTaskManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class TaskManagementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    TaskCategoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.TaskCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskCategoryId);
                    table.ForeignKey(
                        name: "FK_Tasks_Categories_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "Categories",
                        principalColumn: "TaskCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "TaskCategoryId", "Description", "Name" },
                values: new object[] { 100L, "", "Work" });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskCategoryId", "Description", "DueDate", "Priority", "Status", "TaskId", "Title" },
                values: new object[] { 100L, "Complete the quarterly financial report", new DateTime(2024, 10, 18, 16, 34, 39, 744, DateTimeKind.Local).AddTicks(9864), "Low", "Completed", new Guid("4db9015d-fbdd-4f5b-8617-3d314cb79ddc"), "Submit report" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
