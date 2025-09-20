using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Enums;
using Monitoring.Infrastructure.Fakes;
using Monitoring.ServiceEndpoints;
using Monitoring.ServiceEndpoints.HealthChecks;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Xunit;

namespace Monitoring.HealthChecks;

public class HttpHealthCheckTests : MonitoringApplicationTestBase<MonitoringApplicationTestModule>
{
    private readonly HttpHealthCheck _healthCheck;
    private readonly FakeHttpMessageHandler _handler;

    public HttpHealthCheckTests()
    {
        _healthCheck = GetRequiredService<HttpHealthCheck>();
        _handler = GetRequiredService<FakeHttpMessageHandler>();
    }

    [Fact]
    public async Task Should_Report_Healthy_When_Status_Matches()
    {
        _handler.SetResponseFactory((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            ReasonPhrase = "OK"
        }));

        var endpoint = CreateHttpEndpoint("https://example.com", expectedStatus: 200);

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Healthy);
        responseTime.ShouldNotBeNull();
        message.ShouldContain("HTTP 200");
    }

    [Fact]
    public async Task Should_Report_Degraded_When_Status_Does_Not_Match()
    {
        _handler.SetResponseFactory((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            ReasonPhrase = "ServerError"
        }));

        var endpoint = CreateHttpEndpoint("https://example.com", expectedStatus: 200);

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Degraded);
        responseTime.ShouldNotBeNull();
        message.ShouldContain("Expected status 200");
    }

    [Fact]
    public async Task Should_Report_Unhealthy_On_Timeout()
    {
        _handler.SetResponseFactory((_, _) => throw new TaskCanceledException());

        var endpoint = CreateHttpEndpoint("https://example.com", expectedStatus: 200);

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Unhealthy);
        responseTime.ShouldBeNull();
        message.ShouldContain("timed out", StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceEndpoint CreateHttpEndpoint(string target, int expectedStatus)
    {
        var endpoint = new ServiceEndpoint(
            Guid.NewGuid(),
            "Http Endpoint",
            MonitoringServiceType.Http,
            target,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(10));

        endpoint.SetProperty("HttpExpectedStatusCode", expectedStatus);
        return endpoint;
    }
}
