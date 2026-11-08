using ErpLite.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace ErpLite.Api.IntegrationTests;

/// <summary>
/// Runs the real API in memory against a real, throw-away SQL Server database.
/// - Default (CI, or any machine with Docker): a SQL Server container via Testcontainers.
/// - Alternative: set ERPLITE_TEST_CONNECTION (e.g. LocalDB) to use an existing instance;
///   a uniquely named database is created and dropped for the run.
/// </summary>
public sealed class ErpLiteApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ExternalConnectionVariable = "ERPLITE_TEST_CONNECTION";

    private MsSqlContainer? _container;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable(ExternalConnectionVariable);
        if (!string.IsNullOrWhiteSpace(external))
        {
            _connectionString = new SqlConnectionStringBuilder(external)
            {
                InitialCatalog = $"ErpLite_Test_{Guid.NewGuid():N}",
                TrustServerCertificate = true,
            }.ConnectionString;
        }
        else
        {
            _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await _container.StartAsync();
            _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = "ErpLite_Test",
            }.ConnectionString;
        }

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ErpLiteDbContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ErpLite", _connectionString);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
        else
        {
            await using var scope = Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ErpLiteDbContext>();
            await db.Database.EnsureDeletedAsync();
        }
        await base.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ErpLiteApiFactory>
{
    public const string Name = "api";
}
