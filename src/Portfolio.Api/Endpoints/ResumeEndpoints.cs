using Portfolio.Shared;
using Portfolio.Shared.Models;

namespace Portfolio.Api.Endpoints;

public static class ResumeEndpoints
{
    public static IEndpointRouteBuilder MapResumeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Resume");

        group.MapGet("/resume", () => Results.Ok(ResumeData.Current))
             .WithName("GetResume")
             .WithSummary("The full resume graph rendered by the portfolio site.")
             .Produces<Resume>();

        group.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTimeOffset.UtcNow }))
             .WithName("GetHealth")
             .WithSummary("Liveness probe.");

        return app;
    }
}
