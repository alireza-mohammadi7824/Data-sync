using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Monitoring.ServiceEndpoints;

public class ServiceEndpointManager : DomainService
{
    public virtual Task<IReadOnlyList<ServiceEndpoint>> GetDueForCheckAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ServiceEndpoint>>(Array.Empty<ServiceEndpoint>());
    }
}
