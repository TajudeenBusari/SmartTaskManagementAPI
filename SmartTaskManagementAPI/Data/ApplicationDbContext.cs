using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.AppUser;
using SmartTaskManagementAPI.AppUser.models;

namespace SmartTaskManagementAPI.Data;
using TaskManagement.model;
using TaskCategory.model;

//public class ApplicationDbContext: DbContext
public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        :base(options)
    {
    }
    
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
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
        
        //seed users
        
        //AdminUser
        var adminHasher = new PasswordHasher<ApplicationUser>();
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser()
            {
                Id = new Guid("d5968f05-ba4e-4b33-a529-c3b3c281909f").ToString(), // Primary key,
                UserName = "admin",
                FirstName = "Admin1",
                LastName = "Admin1",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = adminHasher.HashPassword(new ApplicationUser(), "Admin123!")
            });
        
        //NormalUser
        var userHasher = new PasswordHasher<ApplicationUser>();
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser()
            {
                Id = new Guid("61616161-6161-6161-6161-616161616161").ToString(), // Primary key,
                UserName = "user",
                FirstName = "John",
                LastName = "Doe",
                NormalizedUserName = "USER",
                Email = "user@example.com",
                NormalizedEmail = "USER@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = userHasher.HashPassword(new ApplicationUser(), "User123!")
            });
        
        // Seed Roles
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN", },
            new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
        );
        
        // Assign roles to admin and user
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>()
            {
                RoleId = "1", //Admin role
                UserId = new Guid("d5968f05-ba4e-4b33-a529-c3b3c281909f").ToString(), // Admin user id
                
            },
            new IdentityUserRole<string>()
            {
                RoleId = "2", //user role
                UserId = new Guid("d5968f05-ba4e-4b33-a529-c3b3c281909f").ToString() //Admin user id
            }
        );
        
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>()
            {
                RoleId = "2", //User role
                UserId = new Guid("61616161-6161-6161-6161-616161616161").ToString() // User role to regular user
            }
        );
        

    }
}

