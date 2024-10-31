using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementIntegrationTests.Container;
using Testcontainers.MsSql;

namespace SmartTaskManagementIntegrationTests.ApplicationFactory;
using SmartTaskManagementAPI.TaskManagement.model;
using SmartTaskManagementAPI.TaskCategory.model;


public class CustomWebApplicationFactory<TProgram>: WebApplicationFactory<TProgram> where TProgram : class
{
    
    private readonly TestDatabaseContainer _testDatabaseContainer;
    public CustomWebApplicationFactory()
    {
        _testDatabaseContainer = new TestDatabaseContainer();
    }
    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing ApplicationDbContext registration
            var descriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            // Register ApplicationDbContext with SQL Server Testcontainer
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_testDatabaseContainer.ConnectionString);
            });

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                //seed data
                SeedTestData(db);


            }
        });
        
    }
    
    private void SeedTestData(ApplicationDbContext context)
    {
        // Example: Seed data if the database is empty
        if (!context.Tasks.Any())
        {
            context.Tasks.AddRange(
                new TaskManagement()
                {
                    TaskId = new Guid("212bd5f9-d108-4dc4-9ef4-03fbe237ad74"),
                    Title = "Sample Task1", 
                    Status = "Open",
                    Description = "description1",
                    DueDate = DateTime.Now,
                    Priority = "High",
                    TaskCategory = new TaskCategory()
                    {
                        Name = ""
                    },
                    TaskCategoryId = 100L
                },
                new TaskManagement()
                {
                    TaskId = new Guid("3fc52bc0-70d2-4301-89c1-f6cbd271cae8"),
                    Title = "Sample Task2", 
                    Status = "Close",
                    Description = "description1 and description1",
                    DueDate = DateTime.Now,
                    Priority = "High",
                    TaskCategory = new TaskCategory()
                    {
                        Name = ""
                    },
                    TaskCategoryId = 200L
                }
            );
            context.SaveChanges();
        }
    }
    
    
    
    public new void Dispose()
    {
        _testDatabaseContainer.Dispose();
        base.Dispose();
    }
}
