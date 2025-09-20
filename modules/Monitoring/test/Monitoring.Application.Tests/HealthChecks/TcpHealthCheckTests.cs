using System;
using System.Net.Sockets;
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

public class TcpHealthCheckTests : MonitoringApplicationTestBase<MonitoringApplicationTestModule>
{
    private readonly TcpHealthCheck _healthCheck;
    private readonly FakeTcpConnector _connector;

    public TcpHealthCheckTests()
    {
        _healthCheck = GetRequiredService<TcpHealthCheck>();
        _connector = GetRequiredService<FakeTcpConnector>();
    }

    [Fact]
    public async Task Should_Return_Healthy_When_Connection_Succeeds()
    {
        _connector.SetBehavior((_, _, _) => Task.CompletedTask);

        var endpoint = CreateTcpEndpoint("localhost:1234");

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Healthy);
        responseTime.ShouldNotBeNull();
        message.ShouldContain("Connected");
    }

    [Fact]
    public async Task Should_Return_Unhealthy_When_Connection_Fails()
    {
        _connector.SetBehavior((_, _, _) => throw new SocketException());

        var endpoint = CreateTcpEndpoint("localhost:1234");

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Unhealthy);
        responseTime.ShouldBeNull();
        message.ShouldContain("TCP socket error");
    }

    [Fact]
    public async Task Should_Validate_Target_Format()
    {
        var endpoint = new ServiceEndpoint(
            Guid.NewGuid(),
            "Invalid TCP",
            MonitoringServiceType.Tcp,
            "invalid",
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(10));

        var (status, responseTime, message) = await _healthCheck.CheckAsync(endpoint, CancellationToken.None);

        status.ShouldBe(MonitoringStatus.Unhealthy);
        responseTime.ShouldBeNull();
        message.ShouldContain("format", StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceEndpoint CreateTcpEndpoint(string target)
    {
        var endpoint = new ServiceEndpoint(
            Guid.NewGuid(),
            "TCP Endpoint",
            MonitoringServiceType.Tcp,
            target,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(10));

        endpoint.SetProperty("TcpHost", "localhost");
        endpoint.SetProperty("TcpPort", 1234);
        return endpoint;
    }
}
