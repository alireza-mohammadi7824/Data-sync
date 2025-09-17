using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monitoring.ServiceEndpoints;

public interface IHealthCheckAppService
{
    Task<RunCheckResultDto> RunCheckAsync(Guid id);

    Task<List<ServiceStatusSnapshotDto>> GetHistoryAsync(Guid id, int take = 100);
}
