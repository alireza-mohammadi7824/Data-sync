using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Enums;
using Volo.Abp.ObjectExtending;

namespace Monitoring.ServiceEndpoints.HealthChecks;

public class HttpHealthCheck : IHealthCheckStrategy, IProvidesResultCode
{
    private const string ExpectedStatusCodePropertyName = "HttpExpectedStatusCode";

    private readonly IHttpClientFactory _httpClientFactory;
    private int? _resultCode;

    public HttpHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public int? ResultCode => _resultCode;

    public async Task<(MonitoringStatus status, int? responseTimeMs, string message)> CheckAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken)
    {
        _resultCode = null;

        if (string.IsNullOrWhiteSpace(endpoint.Target))
        {
            return (MonitoringStatus.Unhealthy, null, "Target is not configured.");
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint.Target);

            var expectedStatus = endpoint.GetProperty<int?>(ExpectedStatusCodePropertyName) ?? 200;

            var client = _httpClientFactory.CreateClient("Monitoring");
            var timeout = endpoint.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.TimeoutSeconds)
                : TimeSpan.FromSeconds(ServiceEndpointConsts.MinTimeoutSeconds);
            client.Timeout = timeout;
            client.Timeout = Timeout.InfiniteTimeSpan;

            var stopwatch = Stopwatch.StartNew();
            using var response = await client.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _resultCode = (int)response.StatusCode;

            var responseTime = (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds);

            if ((int)response.StatusCode == expectedStatus)
            {
                return (MonitoringStatus.Healthy, responseTime, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            return (MonitoringStatus.Degraded, responseTime, $"Expected status {expectedStatus} but received {(int)response.StatusCode}.");
        }
        catch (TaskCanceledException)
        {
            return (MonitoringStatus.Unhealthy, null, "The HTTP request timed out.");
        }
        catch (Exception ex)
        {
            return (MonitoringStatus.Unhealthy, null, $"HTTP check failed: {ex.Message}");
        }
    }
}
