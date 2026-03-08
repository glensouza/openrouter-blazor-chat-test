using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OpenRouterChat.Models;

namespace OpenRouterChat.Services;

public class OpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ILogger<OpenRouterService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenRouterService(HttpClient httpClient, IOptions<OpenRouterSettings> settings, ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> ChatAsync(
        string modelId,
        IEnumerable<ChatMessage> history,
        string userMessage,
        string? contextDocument = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ApiMessage>();

        if (!string.IsNullOrWhiteSpace(contextDocument))
        {
            messages.Add(new ApiMessage
            {
                Role = "system",
                Content = $"You have access to the following document content for context. Use it to answer the user's questions:\n\n---\n{contextDocument}\n---"
            });
        }

        foreach (var msg in history)
        {
            if (!msg.IsError)
                messages.Add(new ApiMessage { Role = msg.Role, Content = msg.Content });
        }

        messages.Add(new ApiMessage { Role = "user", Content = userMessage });

        var request = new ChatRequest
        {
            Model = modelId,
            Messages = messages
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/chat/completions")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        httpRequest.Headers.Add("HTTP-Referer", _settings.HttpReferer);
        httpRequest.Headers.Add("X-Title", _settings.AppTitle);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request to OpenRouter failed");
            throw new InvalidOperationException($"Failed to reach OpenRouter API: {ex.Message}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenRouter API error {Status}: {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"OpenRouter API returned {(int)response.StatusCode}: {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, cancellationToken);
        return result?.Choices?.FirstOrDefault()?.Message?.Content
               ?? throw new InvalidOperationException("Empty response from OpenRouter API.");
    }

    private sealed class ChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<ApiMessage> Messages { get; set; } = [];
    }

    private sealed class ApiMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        public ApiMessage? Message { get; set; }
    }
}
