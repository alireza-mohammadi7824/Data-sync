using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Enums;
using Volo.Abp.ObjectExtending;

namespace Monitoring.ServiceEndpoints.HealthChecks;

public class TcpHealthCheck : IHealthCheckStrategy
{
    private const string HostPropertyName = "TcpHost";
    private const string PortPropertyName = "TcpPort";

    private readonly ITcpConnector _tcpConnector;

    public TcpHealthCheck(ITcpConnector tcpConnector)
    {
        _tcpConnector = tcpConnector;
    }

    public async Task<(MonitoringStatus status, int? responseTimeMs, string message)> CheckAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken)
    {
        if (!TryResolveHostAndPort(endpoint, out var host, out var port, out var validationMessage))
        {
            return (MonitoringStatus.Unhealthy, null, validationMessage ?? "TCP endpoint configuration is invalid.");
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            await _tcpConnector.ConnectAsync(host, port, cancellationToken);
            using var client = new TcpClient();
            var stopwatch = Stopwatch.StartNew();
            await client.ConnectAsync(host, port, cancellationToken);
            stopwatch.Stop();

            var responseTime = (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds);
            return (MonitoringStatus.Healthy, responseTime, $"Connected to {host}:{port} successfully.");
        }
        catch (TaskCanceledException)
        {
            return (MonitoringStatus.Unhealthy, null, "The TCP connection attempt timed out.");
        }
        catch (SocketException ex)
        {
            return (MonitoringStatus.Unhealthy, null, $"TCP socket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (MonitoringStatus.Unhealthy, null, $"TCP check failed: {ex.Message}");
        }
    }

    private static bool TryResolveHostAndPort(ServiceEndpoint endpoint, out string host, out int port, out string? validationMessage)
    {
        host = string.Empty;
        port = 0;
        validationMessage = null;

        host = endpoint.GetProperty<string>(HostPropertyName) ?? string.Empty;
        port = endpoint.GetProperty<int?>(PortPropertyName) ?? 0;

        if (!string.IsNullOrWhiteSpace(host) && port > 0)
        {
            host = host.Trim();
            return true;
        }

        var target = endpoint.Target?.Trim();
        if (string.IsNullOrWhiteSpace(target))
        {
            validationMessage = "TCP target is not configured.";
            return false;
        }

        var separatorIndex = target.LastIndexOf(':');
        if (separatorIndex <= 0 || separatorIndex == target.Length - 1)
        {
            validationMessage = "TCP target must be in the format host:port.";
            return false;
        }

        host = target[..separatorIndex].Trim();
        var portSegment = target[(separatorIndex + 1)..].Trim();
        if (!int.TryParse(portSegment, out port) || port <= 0)
        {
            validationMessage = "TCP port must be a positive integer.";
            return false;
        }

        return true;
    }
}
