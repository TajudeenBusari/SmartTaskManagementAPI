﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartTaskManagementAPI.Data;

#nullable disable

namespace SmartTaskManagementAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SmartTaskManagementAPI.TaskCategory.model.TaskCategory", b =>
                {
                    b.Property<long>("TaskCategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("TaskCategoryId"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TaskCategoryId");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            TaskCategoryId = 100L,
                            Description = "Description1",
                            Name = "Category1"
                        },
                        new
                        {
                            TaskCategoryId = 200L,
                            Description = "Description2",
                            Name = "Category2"
                        });
                });

            modelBuilder.Entity("SmartTaskManagementAPI.TaskManagement.model.TaskManagement", b =>
                {
                    b.Property<Guid>("TaskId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Priority")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("TaskCategoryId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TaskId");

                    b.HasIndex("TaskCategoryId");

                    b.ToTable("Tasks");

                    b.HasData(
                        new
                        {
                            TaskId = new Guid("ee3ef122-af19-4e5f-88c5-f2241ae989c8"),
                            Description = "Complete the quarterly financial report",
                            DueDate = new DateTime(2024, 11, 16, 16, 12, 41, 714, DateTimeKind.Local).AddTicks(4234),
                            Priority = "Low",
                            Status = "in progress",
                            TaskCategoryId = 100L,
                            Title = "Submit report"
                        },
                        new
                        {
                            TaskId = new Guid("6e3be093-5899-49e0-ae87-6759aea2dc1e"),
                            Description = "Buy groceries for the week",
                            DueDate = new DateTime(2024, 11, 17, 16, 12, 41, 714, DateTimeKind.Local).AddTicks(4251),
                            Priority = "Medium",
                            Status = "Pending",
                            TaskCategoryId = 200L,
                            Title = "Grocery Shopping"
                        });
                });

            modelBuilder.Entity("SmartTaskManagementAPI.TaskManagement.model.TaskManagement", b =>
                {
                    b.HasOne("SmartTaskManagementAPI.TaskCategory.model.TaskCategory", "TaskCategory")
                        .WithMany("Tasks")
                        .HasForeignKey("TaskCategoryId");

                    b.Navigation("TaskCategory");
                });

            modelBuilder.Entity("SmartTaskManagementAPI.TaskCategory.model.TaskCategory", b =>
                {
                    b.Navigation("Tasks");
                });
#pragma warning restore 612, 618
        }
    }
}
