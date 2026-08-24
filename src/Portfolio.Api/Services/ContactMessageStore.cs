using System.Text.Json;
using Portfolio.Shared.Models;

namespace Portfolio.Api.Services;

/// <summary>
/// Append-only JSON Lines store for contact-form submissions.
/// Deliberately file-based: a portfolio gets a handful of messages, not a workload that needs a database.
/// Swap the implementation here if you later want email or a real store.
/// </summary>
public sealed class ContactMessageStore(IConfiguration configuration, ILogger<ContactMessageStore> logger)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly string _path = Path.GetFullPath(
        configuration["Contact:StorePath"] ?? Path.Combine(AppContext.BaseDirectory, "data", "messages.jsonl"));

    public async Task SaveAsync(ContactRequest request, string? remoteIp, CancellationToken ct)
    {
        var record = new
        {
            receivedAt = DateTimeOffset.UtcNow,
            remoteIp,
            name = request.Name,
            email = request.Email,
            subject = request.Subject,
            message = request.Message
        };

        await Gate.WaitAsync(ct);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            await File.AppendAllTextAsync(_path, JsonSerializer.Serialize(record) + Environment.NewLine, ct);
        }
        finally
        {
            Gate.Release();
        }

        logger.LogInformation("Contact message stored from {Email} ({Subject})", request.Email, request.Subject ?? "no subject");
    }
}
