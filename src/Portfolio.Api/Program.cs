using Portfolio.Api;
using Portfolio.Api.Endpoints;
using Portfolio.Api.Services;

const string CorsPolicy = "portfolio-site";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<ContactMessageStore>();
builder.Services.AddPortfolioRateLimiting();

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
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.UseRateLimiter();

app.MapResumeEndpoints();
app.MapContactEndpoints();

app.MapGet("/", () => Results.Redirect("/api/health")).ExcludeFromDescription();

app.Run();
