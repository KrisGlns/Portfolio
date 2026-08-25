using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Portfolio.Api;
using Portfolio.Api.Endpoints;
using Portfolio.Api.Services;

const string CorsPolicy = "portfolio-site";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddPortfolioRateLimiting();

// Resend:ApiKey is never committed — set Resend__ApiKey as an environment variable on the host.
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection("Resend"));
builder.Services.AddHttpClient<ResendEmailSender>((sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<ResendOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(15);

    if (!string.IsNullOrWhiteSpace(options.ApiKey))
    {
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
    }
});

// The Blazor client is served from a different origin (GitHub Pages, a static host, or the dev server),
// so the browser needs these origins whitelisted explicitly.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    if (allowedOrigins.Length == 0)
    {
        policy.AllowAnyOrigin();
    }
    else
    {
        policy.WithOrigins(allowedOrigins);
    }

    policy.AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Render (and most container hosts) terminate TLS at the edge and forward plain HTTP,
// so redirecting here would bounce every request.
app.UseCors(CorsPolicy);
app.UseRateLimiter();

app.MapResumeEndpoints();
app.MapContactEndpoints();

app.MapGet("/", () => Results.Redirect("/api/health")).ExcludeFromDescription();

// Surface a misconfiguration at boot rather than on the first visitor's submission.
if (string.IsNullOrWhiteSpace(app.Services.GetRequiredService<IOptions<ResendOptions>>().Value.ApiKey))
{
    app.Logger.LogWarning("Resend__ApiKey is not set — the contact endpoint will return 503.");
}

app.Run();
