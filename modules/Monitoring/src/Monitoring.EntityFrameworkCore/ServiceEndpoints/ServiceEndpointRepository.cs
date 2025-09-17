using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monitoring.ServiceEndpoints;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Monitoring.EntityFrameworkCore.ServiceEndpoints;

public class ServiceEndpointRepository : EfCoreRepository<MonitoringDbContext, ServiceEndpoint, Guid>
{
    public ServiceEndpointRepository(IDbContextProvider<MonitoringDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual Task<IReadOnlyList<ServiceEndpoint>> GetDueForCheckAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ServiceEndpoint>>(Array.Empty<ServiceEndpoint>());
    }
}
