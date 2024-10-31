using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementIntegrationTests.ApplicationFactory;
using SmartTaskManagementIntegrationTests.Container;

namespace SmartTaskManagementIntegrationTests.TaskManagement.SUT;

public class ManagementIntegrationTest: IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ManagementIntegrationTest(CustomWebApplicationFactory<Program> factory)
    {
        var testDatabaseContainer = new TestDatabaseContainer();
        //CustomWebApplicationFactory<Program> applicationFactory = new(testDatabaseContainer);
        _factory = factory;

        // Configure HttpClient
        //var _client = factory.CreateClient();
        
        // Access the DbContext for database cleanup after tests
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
    
    
    private async Task SeedDatabaseWithTasks()
    {
        // Helper to seed test data
        _dbContext.Tasks.Add(new SmartTaskManagementAPI.TaskManagement.model.TaskManagement
        {
            TaskId = Guid.NewGuid(),
            Title = "Sample Task",
            Description = "Sample Task Description",
            DueDate = DateTime.UtcNow,
            Status = "Pending",
            Priority = "Medium"
        });
        await _dbContext.SaveChangesAsync();
    }
 
        
    [Fact]
    public async Task Test1()
    {
        
        var client = _factory.CreateClient();
        
        var res = await client.GetAsync("api/v1/taskmanagement");
        res.EnsureSuccessStatusCode();
    }


    public void Dispose()
    {
        
    }
}