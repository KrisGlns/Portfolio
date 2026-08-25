using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Portfolio.Shared;
using Portfolio.Shared.Models;

namespace Portfolio.Web.Services;

/// <summary>The subset of RFC 7807 validation problem details the API sends back.</summary>
internal sealed record ValidationProblem
{
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; init; }
}

/// <summary>Where the Portfolio.Api lives. Leave the URL blank to run the site fully static.</summary>
public sealed class ApiOptions
{
    public string? BaseUrl { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(BaseUrl);
}

public sealed record SubmitResult(bool Success, string Message, IReadOnlyDictionary<string, string[]>? Errors = null);

/// <summary>
/// Talks to Portfolio.Api. Every read degrades to the compiled-in <see cref="ResumeData"/>, so the
/// published site stays complete and readable even when the API is down or not deployed at all.
/// </summary>
public sealed class PortfolioApiClient(HttpClient http, ApiOptions options, ILogger<PortfolioApiClient> logger)
{
    public bool ContactEnabled => options.IsConfigured;

    /// <summary>
    /// Absolute URL of the health endpoint. The page pings it once when the contact section scrolls
    /// into view, so a sleeping free-tier container is awake by the time anyone presses Send.
    /// </summary>
    public string? HealthUrl => options.IsConfigured
        ? new Uri(new Uri(options.BaseUrl!), "api/health").ToString()
        : null;

    public async Task<Resume> GetResumeAsync(CancellationToken ct = default)
    {
        if (!options.IsConfigured)
        {
            return ResumeData.Current;
        }

        try
        {
            return await http.GetFromJsonAsync<Resume>("api/resume", ct) ?? ResumeData.Current;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Resume API unavailable; falling back to the built-in resume data.");
            return ResumeData.Current;
        }
    }

    public async Task<SubmitResult> SendContactAsync(ContactRequest request, CancellationToken ct = default)
    {
        if (!options.IsConfigured)
        {
            return new SubmitResult(false,
                $"The contact service isn't running right now — please email me directly at {ResumeData.Email}.");
        }

        try
        {
            var response = await http.PostAsJsonAsync("api/contact", request, ct);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<ContactResponse>(ct);
                return new SubmitResult(true, body?.Message ?? "Thanks — your message landed.");
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return new SubmitResult(false, "That's a few messages in a short window. Please try again a little later.");
            }

            // 503 = the API has no Resend key; 502 = Resend refused or was unreachable.
            // Both carry a ContactResponse explaining themselves; append the address so the
            // visitor always leaves with a way to reach you.
            if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway)
            {
                var problem = await response.Content.ReadFromJsonAsync<ContactResponse>(ct);
                logger.LogError("Contact endpoint returned {Status}.", (int)response.StatusCode);
                return new SubmitResult(false,
                    $"{problem?.Message ?? "The contact service is unavailable."} You can reach me at {ResumeData.Email}.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
                return new SubmitResult(false, "Please check the highlighted fields.", problem?.Errors);
            }

            return new SubmitResult(false, $"The server replied with {(int)response.StatusCode}. Please try again shortly.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Contact submission failed.");
            return new SubmitResult(false,
                $"I couldn't reach the contact service. Please email me directly at {ResumeData.Email}.");
        }
    }
}
