using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Portfolio.Web;
using Portfolio.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Api:BaseUrl lives in wwwroot/appsettings.json. Blank means "no backend" — the site then
// renders from the resume data compiled into Portfolio.Shared and hides the contact form.
var api = new ApiOptions { BaseUrl = builder.Configuration["Api:BaseUrl"] };
builder.Services.AddSingleton(api);

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(api.IsConfigured ? api.BaseUrl! : builder.HostEnvironment.BaseAddress),
    Timeout = TimeSpan.FromSeconds(15)
});

builder.Services.AddScoped<PortfolioApiClient>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
