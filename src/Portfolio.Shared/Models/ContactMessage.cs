using System.ComponentModel.DataAnnotations;

namespace Portfolio.Shared.Models;

/// <summary>Payload posted by the contact form. Validated on both sides of the wire.</summary>
public sealed class ContactRequest
{
    [Required(ErrorMessage = "Please tell me your name.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "An email address is needed so I can reply.")]
    [EmailAddress(ErrorMessage = "That does not look like a valid email address.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(120, ErrorMessage = "Subject must be 120 characters or fewer.")]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "The message cannot be empty.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Honeypot: hidden from humans, so any value at all means a bot filled it in.</summary>
    public string? Website { get; set; }
}

public sealed record ContactResponse(bool Accepted, string Message);
