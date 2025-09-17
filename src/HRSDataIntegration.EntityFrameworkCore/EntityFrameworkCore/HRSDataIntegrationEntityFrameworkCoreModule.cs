using System;
using Microsoft.Extensions.DependencyInjection;
using Monitoring.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using HRSDataIntegration.EntityFrameworkCore.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Studio;

namespace HRSDataIntegration.EntityFrameworkCore;

[DependsOn(
    typeof(HRSDataIntegrationDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule),
    typeof(MonitoringEntityFrameworkCoreModule)
    )]
public class HRSDataIntegrationEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {

        HRSDataIntegrationEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.AddAbpDbContext<HRSDataIntegrationDbContext>(options =>
        {
            /* Remove "includeAllEntities: true" to create
             * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }
        context.Services.AddAbpDbContext<HRSDataIntegrationDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });


        context.Services.AddAbpDbContext<OracleDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer(); // Setting SQL Server as the default provider

            options.Configure<HRSDataIntegrationDbContext>(context =>
            {
                context.DbContextOptions.UseSqlServer(configuration.GetConnectionString("Default"));
            });

            options.Configure<OracleDbContext>(context =>
            {
                context.DbContextOptions.UseOracle(configuration.GetConnectionString("OracleDatabase"));
            });
        });

        //Configure<AbpDbContextOptions>(options =>
        //{
        //    /* The main point to change your DBMS.
        //     * See also HRSDataIntegrationDbContextFactory for EF Core tooling. */

        //    options.Use();
        //});


        //collection.AddDbContext<OracleDbContext>(options =>
        //    options.UseOracle(configuration.GetConnectionString("OracleDatabase")));

    }
}
