using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Testcontainers;


namespace SmartTaskManagementIntegrationTests.Container;

public class TestDatabaseContainer: IDisposable
{
    
    private const string Password = "P@ssw0rd123!";
    
    private MsSqlContainer _msSqcontainer;
    

    public TestDatabaseContainer()
    {
        // Configure your Testcontainer SQL Server instance here
        
        
        _msSqcontainer = new MsSqlBuilder()
            .WithPassword(Password)
            
            .WithImage("mcr.microsoft.com/mssql/server")
            .WithCleanUp(true)
            .WithPortBinding(1433, true)
            
            .Build();
        _msSqcontainer.StartAsync();
    }
    
    public string ConnectionString => new SqlConnectionStringBuilder()
    {
        //DataSource = _msSqcontainer.GetConnectionString().Split(';')[0].Split('=')[1],
        DataSource = _msSqcontainer.Hostname,
        UserID = "sa",
        Password = Password,
        InitialCatalog = "SmartTaskManagementAPI",
        IntegratedSecurity = false,
        TrustServerCertificate = true,
        Encrypt = false, // Disable SSL encryption
        MultipleActiveResultSets = true 
        
        
    }.ConnectionString;
    
    public void Dispose()
    {
        _msSqcontainer.DisposeAsync();
    }
}