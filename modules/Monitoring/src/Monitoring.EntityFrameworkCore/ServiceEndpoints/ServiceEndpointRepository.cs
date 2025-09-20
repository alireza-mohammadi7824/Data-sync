using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monitoring.ServiceEndpoints;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Monitoring.EntityFrameworkCore.ServiceEndpoints;

public class ServiceEndpointRepository : EfCoreRepository<MonitoringDbContext, ServiceEndpoint, Guid>, IServiceEndpointRepository
{
    public ServiceEndpointRepository(IDbContextProvider<MonitoringDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<IReadOnlyList<ServiceEndpoint>> GetDueForCheckAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        return await dbContext.ServiceEndpoints
            .AsNoTracking()
            .Where(endpoint => endpoint.IsEnabled)
            .Where(endpoint => endpoint.CheckIntervalSeconds > 0)
            .Where(endpoint => endpoint.LastCheckTime == null ||
                               EF.Functions.DateDiffSecond(endpoint.LastCheckTime.Value, utcNow) >= endpoint.CheckIntervalSeconds)
            .ToListAsync(cancellationToken);
    }
}
