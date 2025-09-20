using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monitoring.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monitoring.Enums;
using Monitoring.Permissions;
using Monitoring.ServiceEndpoints.HealthChecks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Monitoring.ServiceEndpoints;

[Authorize(MonitoringPermissions.Default)]
public class HealthCheckAppService : ApplicationService, IHealthCheckAppService
{
    private readonly IRepository<ServiceEndpoint, Guid> _serviceEndpointRepository;
    private readonly IRepository<ServiceStatusSnapshot, Guid> _statusSnapshotRepository;
    private readonly HealthCheckExecutor _executor;
    private readonly IReadOnlyDictionary<MonitoringServiceType, IHealthCheckStrategy> _strategies;

    public HealthCheckAppService(
        IRepository<ServiceEndpoint, Guid> serviceEndpointRepository,
        IRepository<ServiceStatusSnapshot, Guid> statusSnapshotRepository,
        HealthCheckExecutor executor)
    {
        _serviceEndpointRepository = serviceEndpointRepository;
        _statusSnapshotRepository = statusSnapshotRepository;
        _executor = executor;
        IEnumerable<IHealthCheckStrategy> strategies)
    {
        _serviceEndpointRepository = serviceEndpointRepository;
        _statusSnapshotRepository = statusSnapshotRepository;
        _strategies = BuildStrategyMap(strategies);
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
        var snapshot = await _executor.ExecuteAsync(endpoint);

        return ObjectMapper.Map<ServiceStatusSnapshot, RunCheckResultDto>(snapshot);
        var strategy = GetStrategy(endpoint.ServiceType);
        var now = Clock.Now;

        var timeout = endpoint.TimeoutSeconds > 0
            ? TimeSpan.FromSeconds(endpoint.TimeoutSeconds)
            : TimeSpan.FromSeconds(ServiceEndpointConsts.MinTimeoutSeconds);

        (MonitoringStatus status, int? responseTimeMs, string message) executionResult;

        using var cancellationTokenSource = new CancellationTokenSource(timeout);

        try
        {
            executionResult = await strategy.CheckAsync(endpoint, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            executionResult = (MonitoringStatus.Unhealthy, null, "The health check timed out.");
        }
        catch (Exception ex)
        {
            executionResult = (MonitoringStatus.Unhealthy, null, $"Health check failed: {ex.Message}");
        }

        var duration = executionResult.responseTimeMs.HasValue
            ? TimeSpan.FromMilliseconds(executionResult.responseTimeMs.Value)
            : (TimeSpan?)null;

        var notes = NormalizeNotes(executionResult.message);
        var resultCode = (strategy as IProvidesResultCode)?.ResultCode;

        var snapshot = endpoint.RecordSnapshot(
            GuidGenerator.Create(),
            executionResult.status,
            now,
            duration,
            notes,
            resultCode);

        await _statusSnapshotRepository.InsertAsync(snapshot, autoSave: true);
        await _serviceEndpointRepository.UpdateAsync(endpoint, autoSave: true);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ServiceStatusSnapshot, RunCheckResultDto>(snapshot);
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

    private IHealthCheckStrategy GetStrategy(MonitoringServiceType serviceType)
    {
        if (_strategies.TryGetValue(serviceType, out var strategy))
        {
            return strategy;
        }

        throw new AbpException($"No health check strategy registered for service type '{serviceType}'.");
    }

    private static IReadOnlyDictionary<MonitoringServiceType, IHealthCheckStrategy> BuildStrategyMap(IEnumerable<IHealthCheckStrategy> strategies)
    {
        var map = new Dictionary<MonitoringServiceType, IHealthCheckStrategy>();

        foreach (var strategy in strategies)
        {
            var serviceType = strategy switch
            {
                HttpHealthCheck => MonitoringServiceType.Http,
                ApiHealthCheck => MonitoringServiceType.Api,
                TcpHealthCheck => MonitoringServiceType.Tcp,
                _ => throw new AbpException($"Unknown health check strategy type: {strategy.GetType().FullName}")
            };

            map[serviceType] = strategy;
        }

        return map;
    }

    private static string? NormalizeNotes(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var value = message.Trim();
        if (value.Length <= ServiceStatusSnapshotConsts.MaxNotesLength)
        {
            return value;
        }

        return value[..ServiceStatusSnapshotConsts.MaxNotesLength];
    }
}
