using OpenRouterChat.Components;
using OpenRouterChat.Models;
using OpenRouterChat.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// User secrets (API key stored as "OpenRouter:ApiKey" in developer secrets)
builder.Configuration.AddUserSecrets<Program>(optional: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Bind OpenRouter settings from configuration
builder.Services.Configure<OpenRouterSettings>(builder.Configuration.GetSection("OpenRouter"));

// Register the OpenRouter HTTP client
builder.Services.AddHttpClient<OpenRouterService>((sp, client) =>
{
    ConfigurationManager config = builder.Configuration;
    string baseUrl = config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai";
    string apiKey = config["OpenRouter:ApiKey"] ?? string.Empty;

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
});

// Register document parsing service
builder.Services.AddScoped<DocumentService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
