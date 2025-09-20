using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Enums;
using Monitoring.Infrastructure.Fakes;
using Monitoring.ServiceEndpoints;
using Monitoring.ServiceEndpoints.HealthChecks;
using Shouldly;
using Volo.Abp.ObjectExtending;
using Xunit;

namespace Monitoring.HealthChecks;

public class ApiHealthCheckTests : MonitoringApplicationTestBase<MonitoringApplicationTestModule>
{
    private readonly ApiHealthCheck _healthCheck;
    private readonly FakeHttpMessageHandler _handler;

    public ApiHealthCheckTests()
    {
        _healthCheck = GetRequiredService<ApiHealthCheck>();
        _handler = GetRequiredService<FakeHttpMessageHandler>();
    }

    [Fact]
    public async Task Should_Return_Healthy_When_Json_Value_Matches()
    {
        _handler.SetResponseFactory((_, _) => Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, "{ \"data\": { \"status\": \"ok\" } }")));

        var endpoint = CreateApiEndpoint("https://api", expectedStatus: 200, jsonPath: "data.status", expectedValue: "ok");

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Healthy);
        responseTime.ShouldNotBeNull();
        message.ShouldContain("matched", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Return_Degraded_When_Json_Value_Differs()
    {
        _handler.SetResponseFactory((_, _) => Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, "{ \"data\": { \"status\": \"fail\" } }")));

        var endpoint = CreateApiEndpoint("https://api", expectedStatus: 200, jsonPath: "data.status", expectedValue: "ok");

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Degraded);
        responseTime.ShouldNotBeNull();
        message.ShouldContain("Expected value", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Return_Unhealthy_On_Timeout()
    {
        _handler.SetResponseFactory((_, _) => throw new TaskCanceledException());

        var endpoint = CreateApiEndpoint("https://api", expectedStatus: 200, jsonPath: "data.status", expectedValue: "ok");

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Unhealthy);
        responseTime.ShouldBeNull();
        message.ShouldContain("timed out", StringComparison.OrdinalIgnoreCase);
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
            ReasonPhrase = statusCode.ToString()
        };
        return response;
    }

    private static ServiceEndpoint CreateApiEndpoint(string target, int expectedStatus, string jsonPath, string expectedValue)
    {
        var endpoint = new ServiceEndpoint(
            Guid.NewGuid(),
            "API Endpoint",
            MonitoringServiceType.Api,
            target,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(10));

        endpoint.SetProperty("ApiExpectedStatusCode", expectedStatus);
        endpoint.SetProperty("ApiExpectedJsonPath", jsonPath);
        endpoint.SetProperty("ApiExpectedJsonValue", expectedValue);
        return endpoint;
    }
}
