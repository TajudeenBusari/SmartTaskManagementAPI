using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTaskManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class secondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("4e0e7a49-478e-4d06-a18c-cd23edbf40e2"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "TaskCategoryId",
                keyValue: 100L,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Description1", "Category1" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "TaskCategoryId",
                keyValue: 200L,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Description2", "Category2" });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("ee3ef122-af19-4e5f-88c5-f2241ae989c8"),
                column: "DueDate",
                value: new DateTime(2024, 11, 16, 16, 12, 41, 714, DateTimeKind.Local).AddTicks(4234));

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskId", "Description", "DueDate", "Priority", "Status", "TaskCategoryId", "Title" },
                values: new object[] { new Guid("6e3be093-5899-49e0-ae87-6759aea2dc1e"), "Buy groceries for the week", new DateTime(2024, 11, 17, 16, 12, 41, 714, DateTimeKind.Local).AddTicks(4251), "Medium", "Pending", 200L, "Grocery Shopping" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("6e3be093-5899-49e0-ae87-6759aea2dc1e"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "TaskCategoryId",
                keyValue: 100L,
                columns: new[] { "Description", "Name" },
                values: new object[] { "", "Work" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "TaskCategoryId",
                keyValue: 200L,
                columns: new[] { "Description", "Name" },
                values: new object[] { "", "Personal" });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("ee3ef122-af19-4e5f-88c5-f2241ae989c8"),
                column: "DueDate",
                value: new DateTime(2024, 10, 23, 20, 56, 13, 993, DateTimeKind.Local).AddTicks(6036));

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskId", "Description", "DueDate", "Priority", "Status", "TaskCategoryId", "Title" },
                values: new object[] { new Guid("4e0e7a49-478e-4d06-a18c-cd23edbf40e2"), "Buy groceries for the week", new DateTime(2024, 10, 24, 20, 56, 13, 993, DateTimeKind.Local).AddTicks(6065), "Medium", "Pending", 200L, "Grocery Shopping" });
        }
    }
}
