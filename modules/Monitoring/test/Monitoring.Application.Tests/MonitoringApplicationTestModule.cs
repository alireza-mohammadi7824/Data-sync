using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Monitoring.EntityFrameworkCore;
using Monitoring.Infrastructure.Fakes;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringApplicationModule),
    typeof(MonitoringEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(HRSDataIntegrationTestBaseModule)
)]
public class MonitoringApplicationTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
        });

        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);

        context.Services.AddSingleton<TestClock>();
        context.Services.AddSingleton<Volo.Abp.Timing.IClock>(sp => sp.GetRequiredService<TestClock>());

        context.Services.AddSingleton<FakeHttpMessageHandler>();
        context.Services.Replace(ServiceDescriptor.Singleton<System.Net.Http.IHttpClientFactory>(sp => new TestHttpClientFactory(sp.GetRequiredService<FakeHttpMessageHandler>())));

        context.Services.AddSingleton<FakeTcpConnector>();
        context.Services.Replace(ServiceDescriptor.Transient<ServiceEndpoints.HealthChecks.ITcpConnector>(sp => sp.GetRequiredService<FakeTcpConnector>()));
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _sqliteConnection?.Dispose();
    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(ctx =>
            {
                ctx.DbContextOptions.UseSqlite(_sqliteConnection);
            });
        });
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<MonitoringDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new MonitoringDbContext(options))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}
