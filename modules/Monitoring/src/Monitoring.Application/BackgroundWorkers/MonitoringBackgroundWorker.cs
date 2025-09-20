using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monitoring.Options;
using Monitoring.ServiceEndpoints;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Monitoring.BackgroundWorkers;

public class MonitoringBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ILogger<MonitoringBackgroundWorker> _logger;
    private readonly IClock _clock;

    public MonitoringBackgroundWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MonitoringBackgroundWorker> logger,
        IClock clock)
        : base(timer, serviceScopeFactory)
    {
        _logger = logger;
        _clock = clock;
        Timer.Period = 10_000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var cancellationToken = workerContext.CancellationToken;

        var options = workerContext.ServiceProvider.GetRequiredService<IOptions<MonitoringOptions>>().Value;
        var endpointRepository = workerContext.ServiceProvider.GetRequiredService<IServiceEndpointRepository>();

        var dueEndpoints = await endpointRepository.GetDueForCheckAsync(_clock.Now, cancellationToken);
        if (dueEndpoints.Count == 0)
        {
            return;
        }

        var maxParallelChecks = options.MaxParallelChecks > 0 ? options.MaxParallelChecks : 1;
        using var semaphore = new SemaphoreSlim(maxParallelChecks);

        var endpointIds = dueEndpoints.Select(x => x.Id).ToArray();
        var tasks = endpointIds.Select(id => ProcessEndpointAsync(id, semaphore, cancellationToken)).ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task ProcessEndpointAsync(Guid endpointId, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            using var scope = ServiceScopeFactory.CreateScope();
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            var executor = scope.ServiceProvider.GetRequiredService<HealthCheckExecutor>();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<ServiceEndpoint, Guid>>();

            using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                ServiceEndpoint endpoint;
                try
                {
                    endpoint = await repository.GetAsync(endpointId, includeDetails: true, cancellationToken);
                }
                catch (EntityNotFoundException)
                {
                    return;
                }

                await executor.ExecuteAsync(endpoint, cancellationToken);
                await uow.CompleteAsync();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Swallow cancellations triggered by the host shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing monitoring check for endpoint {EndpointId}.", endpointId);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
