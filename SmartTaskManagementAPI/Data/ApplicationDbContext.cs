using Microsoft.EntityFrameworkCore;

namespace SmartTaskManagementAPI.Data;
using TaskManagement.model;
using TaskCategory.model;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        :base(options)
    {
    }
    
    public DbSet<TaskManagement> Tasks { get; set; }
    public DbSet<TaskCategory> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        //TaskManagement with TaskCategory
        modelBuilder.Entity<TaskManagement>()
            .HasOne(u => u.TaskCategory)
            .WithMany(p => p.Tasks)
            .HasForeignKey(p => p.TaskCategoryId);
        
        modelBuilder.Entity<TaskManagement>().HasData(
            new TaskManagement
        {
            TaskId = new Guid("EE3EF122-AF19-4E5F-88C5-F2241AE989C8"),
            Title = "Submit report",
            Description = "Complete the quarterly financial report",
            DueDate = DateTime.Now,
            Status = "in progress",
            Priority = "Low",
            TaskCategoryId = 100L
            
        },
            new TaskManagement()
            {
                TaskId = Guid.NewGuid(), //15A02374-AF82-4021-A6BD-E24CC442DCA6
                Title = "Grocery Shopping",
                Description = "Buy groceries for the week",
                DueDate = DateTime.Now.AddDays(1),
                Status = "Pending",
                Priority = "Medium",
                TaskCategoryId = 200 // Assigning to the "Personal" category
            }
        );

        //category like "Work," "Personal," Research, Administrative, Operational, Marketing etc
        modelBuilder.Entity<TaskCategory>().HasData(
            new TaskCategory()
        {
            TaskCategoryId = 100L,
            Name = "Category1",
            Description = "Description1",
            
        },
            new TaskCategory()
            {
                TaskCategoryId = 200L,
                Name = "Category2",
                Description = "Description2",
            
            }
        );
    }
}
/*
 * dotnet ef migrations add InitialMigration
* dotnet ef database update
 *
 *
 * using Microsoft.EntityFrameworkCore;
using System;

public class ApplicationDbContext : DbContext
{
    public DbSet<TaskManagement> TaskManagements { get; set; }
    public DbSet<TaskCategory> TaskCategories { get; set; }

    // Constructor
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define the relationship between TaskCategory and TaskManagement
        modelBuilder.Entity<TaskManagement>()
            .HasOne(t => t.TaskCategory)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.TaskCategoryId);

        // Seed data for TaskCategory
        modelBuilder.Entity<TaskCategory>().HasData(
            new TaskCategory
            {
                TaskCategoryId = 1, // Ensure this matches the FK in the TaskManagement
                Name = "Work",
                Description = "Work-related tasks"
            },
            new TaskCategory
            {
                TaskCategoryId = 2,
                Name = "Personal",
                Description = "Personal tasks and to-dos"
            }
        );

        // Seed data for TaskManagement
        modelBuilder.Entity<TaskManagement>().HasData(
            new TaskManagement
            {
                TaskId = Guid.NewGuid(),
                Title = "Submit Report",
                Description = "Complete the quarterly financial report",
                DueDate = DateTime.Now.AddDays(3),
                Status = "Pending",
                Priority = "High",
                TaskCategoryId = 1 // Assigning to the "Work" category
            },
            new TaskManagement
            {
                TaskId = Guid.NewGuid(),
                Title = "Grocery Shopping",
                Description = "Buy groceries for the week",
                DueDate = DateTime.Now.AddDays(1),
                Status = "Pending",
                Priority = "Medium",
                TaskCategoryId = 2 // Assigning to the "Personal" category
            }
        );
    }
}

 */
