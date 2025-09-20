using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Enums;
using Volo.Abp.ObjectExtending;

namespace Monitoring.ServiceEndpoints.HealthChecks;

public class ApiHealthCheck : IHealthCheckStrategy, IProvidesResultCode
{
    private const string MethodPropertyName = "ApiMethod";
    private const string HeadersPropertyName = "ApiHeaders";
    private const string ExpectedStatusCodePropertyName = "ApiExpectedStatusCode";
    private const string ExpectedJsonPathPropertyName = "ApiExpectedJsonPath";
    private const string ExpectedJsonValuePropertyName = "ApiExpectedJsonValue";

    private readonly IHttpClientFactory _httpClientFactory;
    private int? _resultCode;

    public ApiHealthCheck(IHttpClientFactory httpClientFactory)
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
            var methodName = endpoint.GetProperty<string>(MethodPropertyName) ?? HttpMethod.Get.Method;
            var method = new HttpMethod(methodName);
            var request = new HttpRequestMessage(method, endpoint.Target);

            AddHeaders(endpoint, request);

            var expectedStatus = endpoint.GetProperty<int?>(ExpectedStatusCodePropertyName) ?? 200;

            var client = _httpClientFactory.CreateClient("Monitoring");
            var timeout = endpoint.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.TimeoutSeconds)
                : TimeSpan.FromSeconds(ServiceEndpointConsts.MinTimeoutSeconds);
            client.Timeout = timeout;

            var stopwatch = Stopwatch.StartNew();
            using var response = await client.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _resultCode = (int)response.StatusCode;

            var responseTime = (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds);

            if ((int)response.StatusCode != expectedStatus)
            {
                return (MonitoringStatus.Unhealthy, responseTime, $"Expected status {expectedStatus} but received {(int)response.StatusCode}.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
            {
                return (MonitoringStatus.Healthy, responseTime, "API responded with expected status.");
            }

            var expectedJsonPath = endpoint.GetProperty<string>(ExpectedJsonPathPropertyName);
            var expectedJsonValue = endpoint.GetProperty<string>(ExpectedJsonValuePropertyName);

            if (string.IsNullOrWhiteSpace(expectedJsonPath))
            {
                return (MonitoringStatus.Healthy, responseTime, "API responded with expected status.");
            }

            var actualValue = ExtractJsonValue(content, expectedJsonPath);

            if (actualValue == null)
            {
                return (MonitoringStatus.Degraded, responseTime, $"JSON path '{expectedJsonPath}' was not found.");
            }

            if (expectedJsonValue != null && !string.Equals(actualValue, expectedJsonValue, StringComparison.OrdinalIgnoreCase))
            {
                return (MonitoringStatus.Degraded, responseTime, $"Expected JSON value '{expectedJsonValue}' but found '{actualValue}'.");
            }

            return (MonitoringStatus.Healthy, responseTime, "API responded with expected content.");
        }
        catch (TaskCanceledException)
        {
            return (MonitoringStatus.Unhealthy, null, "The API request timed out.");
        }
        catch (Exception ex)
        {
            return (MonitoringStatus.Unhealthy, null, $"API check failed: {ex.Message}");
        }
    }

    private static void AddHeaders(ServiceEndpoint endpoint, HttpRequestMessage request)
    {
        var headers = endpoint.GetProperty<string>(HeadersPropertyName);
        if (string.IsNullOrWhiteSpace(headers))
        {
            return;
        }

        foreach (var header in headers.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = header.Split(':', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            request.Headers.TryAddWithoutValidation(parts[0].Trim(), parts[1].Trim());
        }
    }

    private static string? ExtractJsonValue(string json, string path)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            JsonElement element = document.RootElement;

            foreach (var segment in segments)
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(segment, out var property))
                {
                    element = property;
                    continue;
                }

                return null;
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDouble().ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }
        catch
        {
            return null;
        }
    }
}
