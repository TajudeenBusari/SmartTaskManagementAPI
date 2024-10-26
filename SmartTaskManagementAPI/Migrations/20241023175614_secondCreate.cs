using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartTaskManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class secondCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Categories_TaskCategoryId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskCategoryId",
                keyValue: 100L);

            migrationBuilder.AlterColumn<long>(
                name: "TaskCategoryId",
                table: "Tasks",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "TaskId");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "TaskCategoryId", "Description", "Name" },
                values: new object[] { 200L, "", "Personal" });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskId", "Description", "DueDate", "Priority", "Status", "TaskCategoryId", "Title" },
                values: new object[,]
                {
                    { new Guid("ee3ef122-af19-4e5f-88c5-f2241ae989c8"), "Complete the quarterly financial report", new DateTime(2024, 10, 23, 20, 56, 13, 993, DateTimeKind.Local).AddTicks(6036), "Low", "in progress", 100L, "Submit report" },
                    { new Guid("4e0e7a49-478e-4d06-a18c-cd23edbf40e2"), "Buy groceries for the week", new DateTime(2024, 10, 24, 20, 56, 13, 993, DateTimeKind.Local).AddTicks(6065), "Medium", "Pending", 200L, "Grocery Shopping" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskCategoryId",
                table: "Tasks",
                column: "TaskCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Categories_TaskCategoryId",
                table: "Tasks",
                column: "TaskCategoryId",
                principalTable: "Categories",
                principalColumn: "TaskCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Categories_TaskCategoryId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TaskCategoryId",
                table: "Tasks");

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("4e0e7a49-478e-4d06-a18c-cd23edbf40e2"));

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("ee3ef122-af19-4e5f-88c5-f2241ae989c8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "TaskCategoryId",
                keyValue: 200L);

            migrationBuilder.AlterColumn<long>(
                name: "TaskCategoryId",
                table: "Tasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "TaskCategoryId");

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskCategoryId", "Description", "DueDate", "Priority", "Status", "TaskId", "Title" },
                values: new object[] { 100L, "Complete the quarterly financial report", new DateTime(2024, 10, 18, 16, 34, 39, 744, DateTimeKind.Local).AddTicks(9864), "Low", "Completed", new Guid("4db9015d-fbdd-4f5b-8617-3d314cb79ddc"), "Submit report" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Categories_TaskCategoryId",
                table: "Tasks",
                column: "TaskCategoryId",
                principalTable: "Categories",
                principalColumn: "TaskCategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
