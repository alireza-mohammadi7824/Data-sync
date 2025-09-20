using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Monitoring.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Monitoring.ServiceEndpoints;

[Authorize(MonitoringPermissions.Default)]
public class HealthCheckAppService : ApplicationService, IHealthCheckAppService
{
    private readonly IRepository<ServiceEndpoint, Guid> _serviceEndpointRepository;
    private readonly IRepository<ServiceStatusSnapshot, Guid> _statusSnapshotRepository;
    private readonly HealthCheckExecutor _executor;

    public HealthCheckAppService(
        IRepository<ServiceEndpoint, Guid> serviceEndpointRepository,
        IRepository<ServiceStatusSnapshot, Guid> statusSnapshotRepository,
        HealthCheckExecutor executor)
    {
        _serviceEndpointRepository = serviceEndpointRepository;
        _statusSnapshotRepository = statusSnapshotRepository;
        _executor = executor;
    }

    [Authorize(MonitoringPermissions.RunCheck)]
    public virtual async Task<RunCheckResultDto> RunCheckAsync(Guid id)
    {
        var endpoint = await _serviceEndpointRepository.GetAsync(id);
        var snapshot = await _executor.ExecuteAsync(endpoint);

        return ObjectMapper.Map<ServiceStatusSnapshot, RunCheckResultDto>(snapshot);
    }

    [Authorize(MonitoringPermissions.View)]
    public virtual async Task<List<ServiceStatusSnapshotDto>> GetHistoryAsync(Guid id, int take = 100)
    {
        await _serviceEndpointRepository.GetAsync(id);

        var clampedTake = take <= 0 ? 100 : Math.Min(take, 500);

        var queryable = await _statusSnapshotRepository.GetQueryableAsync();
        var query = queryable
            .Where(x => x.ServiceEndpointId == id)
            .OrderByDescending(x => x.CheckedAt)
            .ThenByDescending(x => x.Id)
            .Take(clampedTake);

        var snapshots = await AsyncExecuter.ToListAsync(query);

        return ObjectMapper.Map<List<ServiceStatusSnapshot>, List<ServiceStatusSnapshotDto>>(snapshots);
    }
}
