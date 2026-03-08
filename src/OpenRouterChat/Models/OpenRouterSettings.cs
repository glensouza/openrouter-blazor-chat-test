namespace OpenRouterChat.Models;

public class OpenRouterSettings
{
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string HttpReferer { get; set; } = "https://localhost";
    public string AppTitle { get; set; } = "OpenRouter Blazor Chat";
    public List<ModelInfo> Models { get; set; } = [];
}

public class ModelInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
