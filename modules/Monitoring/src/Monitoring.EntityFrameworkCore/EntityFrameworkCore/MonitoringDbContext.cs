using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Monitoring.ServiceEndpoints;

namespace Monitoring.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class MonitoringDbContext : AbpDbContext<MonitoringDbContext>, ITransientDependency
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceEndpoint> ServiceEndpoints { get; set; } = default!;

    public DbSet<ServiceStatusSnapshot> ServiceStatusSnapshots { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureMonitoring();
    }
}
