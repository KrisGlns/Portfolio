using System.ComponentModel.DataAnnotations;
using Portfolio.Api.Services;
using Portfolio.Shared.Models;

namespace Portfolio.Api.Endpoints;

public static class ContactEndpoints
{
    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/contact", HandleAsync)
           .WithTags("Contact")
           .WithName("SubmitContactMessage")
           .WithSummary("Accepts a message from the portfolio contact form and emails it on via Resend.")
           .RequireRateLimiting(RateLimitPolicies.Contact)
           .Produces<ContactResponse>()
           .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ContactRequest request,
        ResendEmailSender email,
        HttpContext http,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        // Honeypot: a real browser never sees this field, so anything in it is a bot.
        // Answer 202 anyway so the bot learns nothing from the response.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            loggerFactory.CreateLogger("Contact").LogWarning("Honeypot triggered from {Ip}", http.Connection.RemoteIpAddress);
            return Results.Accepted(value: new ContactResponse(true, "Thanks — your message is on its way."));
        }

        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        // Fail loudly rather than accepting a message nothing will ever deliver.
        if (!email.IsConfigured)
        {
            loggerFactory.CreateLogger("Contact")
                .LogError("Resend is not configured — set Resend__ApiKey and Resend__To. Message dropped from {Email}.", request.Email);

            return Results.Json(
                new ContactResponse(false, "The contact service isn't configured right now — please email me directly."),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (!await email.SendAsync(request, ct))
        {
            return Results.Json(
                new ContactResponse(false, "I couldn't send that just now — please email me directly."),
                statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Ok(new ContactResponse(true, "Thanks — your message landed. I'll get back to you shortly."));
    }

    private static Dictionary<string, string[]> Validate(ContactRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);

        return results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), (r, member) => (member, r.ErrorMessage))
            .GroupBy(x => x.member, x => x.ErrorMessage ?? "Invalid value.")
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
