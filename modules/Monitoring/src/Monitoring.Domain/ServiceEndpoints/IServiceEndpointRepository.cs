using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Monitoring.ServiceEndpoints;

public interface IServiceEndpointRepository : IRepository<ServiceEndpoint, Guid>
{
    Task<IReadOnlyList<ServiceEndpoint>> GetDueForCheckAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
