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
            return (MonitoringStatus.Unhealthy, null, "API target is not configured.");
        }

        try
        {
            var method = ResolveMethod(endpoint.GetProperty<string>(MethodPropertyName));
            var request = new HttpRequestMessage(method, endpoint.Target);

            ApplyHeaders(endpoint, request);

            var expectedStatus = endpoint.GetProperty<int?>(ExpectedStatusCodePropertyName) ?? 200;

            var client = _httpClientFactory.CreateClient("Monitoring");
            client.Timeout = Timeout.InfiniteTimeSpan;

            var stopwatch = Stopwatch.StartNew();
            using var response = await client.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _resultCode = (int)response.StatusCode;

            var responseTime = (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds);

            if ((int)response.StatusCode != expectedStatus)
            {
                return (MonitoringStatus.Degraded, responseTime, $"Expected status {expectedStatus} but received {(int)response.StatusCode}.");
            }

            var expectedPath = endpoint.GetProperty<string>(ExpectedJsonPathPropertyName);
            if (string.IsNullOrWhiteSpace(expectedPath))
            {
                return (MonitoringStatus.Healthy, responseTime, $"API returned {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return (MonitoringStatus.Degraded, responseTime, "Response did not contain content to validate.");
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                if (!TryResolveJsonValue(document.RootElement, expectedPath!, out var actualValue))
                {
                    return (MonitoringStatus.Degraded, responseTime, $"JSON path '{expectedPath}' was not found in the response.");
                }

                var expectedValue = endpoint.GetProperty<string>(ExpectedJsonValuePropertyName) ?? string.Empty;
                if (string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    return (MonitoringStatus.Healthy, responseTime, "API response matched the expected JSON value.");
                }

                return (MonitoringStatus.Degraded, responseTime, $"Expected value '{expectedValue}' at path '{expectedPath}' but found '{actualValue}'.");
            }
            catch (JsonException)
            {
                return (MonitoringStatus.Degraded, responseTime, "Response content was not valid JSON.");
            }
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

    private static HttpMethod ResolveMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return HttpMethod.Get;
        }

        return method.Trim().ToUpperInvariant() switch
        {
            "POST" => HttpMethod.Post,
            _ => HttpMethod.Get,
        };
    }

    private static void ApplyHeaders(ServiceEndpoint endpoint, HttpRequestMessage request)
    {
        var headersValue = endpoint.GetProperty<string>(HeadersPropertyName);
        if (string.IsNullOrWhiteSpace(headersValue))
        {
            return;
        }

        var headerPairs = headersValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in headerPairs)
        {
            var parts = pair.Split(':', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var headerName = parts[0].Trim();
            var headerValue = parts[1].Trim();
            if (string.IsNullOrEmpty(headerName) || string.IsNullOrEmpty(headerValue))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(headerName, headerValue))
            {
                request.Content ??= new StringContent(string.Empty);
                request.Content.Headers.TryAddWithoutValidation(headerName, headerValue);
            }
        }
    }

    private static bool TryResolveJsonValue(JsonElement element, string path, out string actualValue)
    {
        actualValue = string.Empty;
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        var current = element;
        foreach (var segment in segments)
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!current.TryGetProperty(segment, out var property))
                {
                    return false;
                }

                current = property;
                continue;
            }

            if (current.ValueKind == JsonValueKind.Array && int.TryParse(segment, out var index))
            {
                if (index < 0 || index >= current.GetArrayLength())
                {
                    return false;
                }

                current = current[index];
                continue;
            }

            return false;
        }

        actualValue = current.ValueKind switch
        {
            JsonValueKind.String => current.GetString() ?? string.Empty,
            JsonValueKind.Number => current.ToString(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null => string.Empty,
            _ => current.ToString()
        };

        return true;
    }
}
