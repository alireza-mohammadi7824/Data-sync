using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monitoring.BackgroundWorkers;
using Monitoring.Enums;
using Monitoring.Infrastructure.Fakes;
using Monitoring.ServiceEndpoints;
using Monitoring.ServiceEndpoints.HealthChecks;
using Shouldly;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Xunit;

namespace Monitoring.BackgroundWorkersTests;

public class MonitoringBackgroundWorkerTests : MonitoringApplicationTestBase<MonitoringApplicationTestModule>
{
    private readonly TestClock _clock;
    private readonly FakeHttpMessageHandler _httpHandler;
    private readonly IRepository<ServiceEndpoint, Guid> _endpointRepository;
    private readonly IRepository<ServiceStatusSnapshot, Guid> _snapshotRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitoringBackgroundWorker> _logger;
    private readonly IGuidGenerator _guidGenerator;

    public MonitoringBackgroundWorkerTests()
    {
        _clock = GetRequiredService<TestClock>();
        _httpHandler = GetRequiredService<FakeHttpMessageHandler>();
        _endpointRepository = GetRequiredService<IRepository<ServiceEndpoint, Guid>>();
        _snapshotRepository = GetRequiredService<IRepository<ServiceStatusSnapshot, Guid>>();
        _scopeFactory = GetRequiredService<IServiceScopeFactory>();
        _logger = GetRequiredService<ILogger<MonitoringBackgroundWorker>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Should_Execute_Only_Due_Endpoints()
    {
        _httpHandler.Reset();

        var now = new DateTime(2024, 5, 10, 12, 0, 0, DateTimeKind.Utc);
        _clock.SetNow(now);

        var dueEndpoint = new ServiceEndpoint(
            Guid.NewGuid(),
            "Due",
            MonitoringServiceType.Http,
            "https://due",
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(10));

        var notDueEndpoint = new ServiceEndpoint(
            Guid.NewGuid(),
            "Not Due",
            MonitoringServiceType.Http,
            "https://recent",
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(10));

        notDueEndpoint.RecordSnapshot(
            _guidGenerator.Create(),
            MonitoringStatus.Healthy,
            now.Subtract(TimeSpan.FromSeconds(10)),
            TimeSpan.FromMilliseconds(100),
            "seed");

        await WithUnitOfWorkAsync(async () =>
        {
            await _endpointRepository.InsertAsync(dueEndpoint);
            await _endpointRepository.InsertAsync(notDueEndpoint);
        });

        _httpHandler.SetResponseFactory((request, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            ReasonPhrase = "OK"
        }));

        var timer = new AbpAsyncTimer { Period = 1_000 };
        var worker = new TestMonitoringBackgroundWorker(timer, _scopeFactory, _logger, _clock);

        await worker.ExecuteOnceAsync(ServiceProvider, CancellationToken.None);

        await WithUnitOfWorkAsync(async () =>
        {
            var dueSnapshots = await _snapshotRepository.GetListAsync(x => x.ServiceEndpointId == dueEndpoint.Id);
            var notDueSnapshots = await _snapshotRepository.GetListAsync(x => x.ServiceEndpointId == notDueEndpoint.Id);

            dueSnapshots.Count.ShouldBe(1);
            notDueSnapshots.Count.ShouldBe(1);

            var updatedDueEndpoint = await _endpointRepository.GetAsync(dueEndpoint.Id);
            updatedDueEndpoint.LastCheckTime.ShouldNotBeNull();
            updatedDueEndpoint.LastCheckTime.Value.ShouldBe(now);
        });

        _httpHandler.Requests.Count.ShouldBe(1);
        _httpHandler.Requests[0].RequestUri.ShouldBe(new Uri("https://due"));
    }

    private class TestMonitoringBackgroundWorker : MonitoringBackgroundWorker
    {
        public TestMonitoringBackgroundWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MonitoringBackgroundWorker> logger,
            IClock clock)
            : base(timer, serviceScopeFactory, logger, clock)
        {
        }

        public Task ExecuteOnceAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var context = new PeriodicBackgroundWorkerContext(serviceProvider, cancellationToken);
            return DoWorkAsync(context);
        }
    }
}
