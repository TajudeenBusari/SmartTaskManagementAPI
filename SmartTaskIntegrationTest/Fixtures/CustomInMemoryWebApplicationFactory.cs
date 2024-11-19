using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartTaskManagementAPI.Data;

namespace SmartTaskIntegrationTest.Fixtures;

public class CustomInMemoryWebApplicationFactory<TProgram>: WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            //Remove actual database
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            services.Remove(descriptor);
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
            services.Remove(dbContextDescriptor);
            
            //Replace database with in memory database
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<ApplicationDbContext>((container, options) =>
            {
                var connection =container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);

            });
        });
        
        builder.UseEnvironment("Development");
    }
    
    
}