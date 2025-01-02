
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartTaskManagementAPI.Data;
using Testcontainers.MsSql;

namespace SmartTaskIntegrationTest.Fixtures;

public class CustomDockerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    public CustomDockerWebApplicationFactory()
    {
        //create container for the SQL Server
        
         _dbContainer = new MsSqlBuilder()
             .WithImage("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04")
             .Build();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _dbContainer.GetConnectionString();
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

        });

    }
    
    
    //initialize container
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using (var scope = Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            //if there are already data in the real DB, there is no need to create new data and save
            await db.Tasks.AddRangeAsync(DataFixture.GetTaskManagement());
            //await db.Categories.AddRangeAsync(DataFixture.GetTaskCategories());
            await db.SaveChangesAsync();
        }
    }

    //stop container
    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}

//test container Doc: https://testcontainers.com/modules/mssql/?language=dotnet