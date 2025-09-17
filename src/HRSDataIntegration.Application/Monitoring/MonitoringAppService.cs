using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRSDataIntegration.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace HRSDataIntegration.Monitoring;

public class MonitoringAppService : HRSDataIntegrationAppService, IMonitoringAppService
{
    private static readonly object SyncObj = new();
    private static readonly List<MonitoringServiceDto> Services = new();

    public Task<MonitoringDashboardDto> GetDashboardAsync()
    {
        List<MonitoringServiceDto> services;

        lock (SyncObj)
        {
            services = Services
                .Select(service => new MonitoringServiceDto
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    IsActive = service.IsActive,
                    LastCheckTime = service.LastCheckTime
                })
                .OrderBy(s => s.Name)
                .ToList();
        }

        var summary = new MonitoringSummaryDto
        {
            TotalServices = services.Count,
            ActiveServices = services.Count(s => s.IsActive),
            GeneratedAt = Clock.Now
        };

        var dashboard = new MonitoringDashboardDto
        {
            Summary = summary,
            Services = services
        };

        return Task.FromResult(dashboard);
    }

    [Authorize(HRSDataIntegrationPermissions.Monitoring.Services.Create)]
    public Task<MonitoringServiceDto> CreateAsync(CreateMonitoringServiceDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var name = input.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Service name is required", nameof(input));
        }

        var service = new MonitoringServiceDto
        {
            Id = GuidGenerator.Create(),
            Name = name,
            Description = string.IsNullOrWhiteSpace(input.Description)
                ? null
                : input.Description.Trim(),
            IsActive = input.IsActive,
            LastCheckTime = null
        };

        lock (SyncObj)
        {
            Services.Add(service);
        }

        return Task.FromResult(service);
    }
}
