using Microsoft.EntityFrameworkCore;
using Monitoring.ServiceEndpoints;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Monitoring.EntityFrameworkCore;

public static class MonitoringDbContextModelCreatingExtensions
{
    public static void ConfigureMonitoring(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ServiceEndpoint>(b =>
        {
            b.ToTable("AppServiceEndpoints");
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(ServiceEndpointConsts.MaxNameLength);

            b.Property(x => x.Description)
                .HasMaxLength(ServiceEndpointConsts.MaxDescriptionLength);

            b.Property(x => x.Target)
                .IsRequired()
                .HasMaxLength(ServiceEndpointConsts.MaxTargetLength);

            b.Property(x => x.CheckIntervalSeconds)
                .IsRequired();

            b.Property(x => x.TimeoutSeconds)
                .IsRequired();

            b.Property(x => x.LastKnownStatus)
                .IsRequired();

            b.Property(x => x.LastCheckTime);

            b.Property(x => x.LastResponseDurationMilliseconds);

            b.HasMany<ServiceStatusSnapshot>()
                .WithOne()
                .HasForeignKey(x => x.ServiceEndpointId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.StatusSnapshots)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            b.HasIndex(x => new { x.ServiceType, x.IsEnabled });
        });

        builder.Entity<ServiceStatusSnapshot>(b =>
        {
            b.ToTable("AppServiceStatusSnapshots");
            b.ConfigureByConvention();

            b.Property(x => x.Notes)
                .HasMaxLength(ServiceStatusSnapshotConsts.MaxNotesLength);

            b.HasIndex(x => new { x.ServiceEndpointId, x.CheckedAt })
                .IsDescending(false, true);
        });
    }
}
