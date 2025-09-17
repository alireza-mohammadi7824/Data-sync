using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monitoring.Enums;
using Monitoring.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Monitoring.ServiceEndpoints;

[Authorize(MonitoringPermissions.Default)]
public class HealthCheckAppService : ApplicationService
{
    private readonly IRepository<ServiceEndpoint, Guid> _serviceEndpointRepository;
    private readonly IRepository<ServiceStatusSnapshot, Guid> _statusSnapshotRepository;

    public HealthCheckAppService(
        IRepository<ServiceEndpoint, Guid> serviceEndpointRepository,
        IRepository<ServiceStatusSnapshot, Guid> statusSnapshotRepository)
    {
        _serviceEndpointRepository = serviceEndpointRepository;
        _statusSnapshotRepository = statusSnapshotRepository;
    }

    [Authorize(MonitoringPermissions.RunCheck)]
    public virtual async Task<RunCheckResultDto> RunCheckAsync(Guid id)
    {
        var endpoint = await _serviceEndpointRepository.GetAsync(id);
        var now = Clock.Now;

        return new RunCheckResultDto
        {
            ServiceEndpointId = endpoint.Id,
            Status = MonitoringStatus.Unknown,
            CheckedAt = now
        };
    }

    [Authorize(MonitoringPermissions.View)]
    public virtual async Task<List<ServiceStatusSnapshotDto>> GetHistoryAsync(Guid id, int take = 100)
    {
        await _serviceEndpointRepository.GetAsync(id);

        var clampedTake = take <= 0 ? 100 : Math.Min(take, 500);

        var queryable = await _statusSnapshotRepository.GetQueryableAsync();
        var snapshots = await queryable
            .Where(x => x.ServiceEndpointId == id)
            .OrderByDescending(x => x.CheckedAt)
            .ThenByDescending(x => x.Id)
            .Take(clampedTake)
            .ToListAsync();

        return ObjectMapper.Map<List<ServiceStatusSnapshot>, List<ServiceStatusSnapshotDto>>(snapshots);
    }
}
