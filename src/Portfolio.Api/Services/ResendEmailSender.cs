using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Portfolio.Shared.Models;

namespace Portfolio.Api.Services;

public sealed class ResendOptions
{
    /// <summary>Never committed. Set as the environment variable <c>Resend__ApiKey</c> on the host.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Override only to test against a stub, or to point at an API-compatible provider.</summary>
    public string BaseUrl { get; set; } = "https://api.resend.com/";

    /// <summary>Resend's sandbox sender works without a verified domain.</summary>
    public string From { get; set; } = "Portfolio <onboarding@resend.dev>";

    /// <summary>Must be the Resend account's own address while sending from the sandbox domain.</summary>
    public string To { get; set; } = string.Empty;

    public string SubjectPrefix { get; set; } = "Portfolio";
}

/// <summary>
/// Sends contact-form messages through Resend's REST API — one authenticated POST, no SDK.
/// The visitor's address goes in Reply-To, so replying from the inbox reaches them directly.
/// </summary>
public sealed class ResendEmailSender(
    HttpClient http,
    IOptions<ResendOptions> options,
    ILogger<ResendEmailSender> logger)
{
    private readonly ResendOptions _options = options.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiKey) && !string.IsNullOrWhiteSpace(_options.To);

    public async Task<bool> SendAsync(ContactRequest request, CancellationToken ct)
    {
        var payload = new
        {
            from = _options.From,
            to = new[] { _options.To },
            reply_to = request.Email,
            subject = $"{_options.SubjectPrefix} — {Fallback(request.Subject, "New message")}",
            text = BuildBody(request)
        };

        try
        {
            var response = await http.PostAsJsonAsync("emails", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Contact message from {Email} sent via Resend.", request.Email);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            // Log the whole message: the container filesystem is ephemeral, so the log is the only
            // place a failed submission survives.
            logger.LogError("Resend rejected the message ({Status}): {Body}\nUnsent message:\n{Message}",
                (int)response.StatusCode, body, BuildBody(request));
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not reach Resend.\nUnsent message:\n{Message}", BuildBody(request));
            return false;
        }
    }

    private static string BuildBody(ContactRequest request) =>
        $"""
         From:    {request.Name} <{request.Email}>
         Subject: {Fallback(request.Subject, "(none)")}

         {request.Message}
         """;

    private static string Fallback(string? value, string whenEmpty) =>
        string.IsNullOrWhiteSpace(value) ? whenEmpty : value.Trim();
}
