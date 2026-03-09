using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OpenRouterChat.Models;

namespace OpenRouterChat.Services;

public class OpenRouterService(HttpClient httpClient, IOptions<OpenRouterSettings> settings, ILogger<OpenRouterService> logger)
{
    private readonly OpenRouterSettings settings = settings.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<ChatResult> ChatAsync(
        string modelId,
        IEnumerable<ChatMessage> history,
        string userMessage,
        string? contextDocument = null,
        CancellationToken cancellationToken = default)
    {
        List<ApiMessage> messages = [];

        if (!string.IsNullOrWhiteSpace(contextDocument))
        {
            messages.Add(new ApiMessage
            {
                Role = "system",
                Content = $"You have access to the following document content for context. Use it to answer the user's questions:\n\n---\n{contextDocument}\n---"
            });
        }

        foreach (ChatMessage msg in history)
        {
            if (!msg.IsError)
                messages.Add(new ApiMessage { Role = msg.Role, Content = msg.Content });
        }

        messages.Add(new ApiMessage { Role = "user", Content = userMessage });

        ChatRequest request = new()
        {
            Model = modelId,
            Messages = messages
        };

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "/api/v1/chat/completions");
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);
        httpRequest.Headers.Add("HTTP-Referer", this.settings.HttpReferer);
        httpRequest.Headers.Add("X-Title", this.settings.AppTitle);

        DateTime startTime = DateTime.UtcNow;
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(httpRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP request to OpenRouter failed");
            throw new InvalidOperationException($"Failed to reach OpenRouter API: {ex.Message}", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("OpenRouter API error {Status}: {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"OpenRouter API returned {(int)response.StatusCode}: {errorBody}");
            }

            ChatResponse? result = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, cancellationToken);
            TimeSpan duration = DateTime.UtcNow - startTime;
            string content = result?.Choices?.FirstOrDefault()?.Message?.Content
                             ?? throw new InvalidOperationException("Empty response from OpenRouter API.");
            Usage usage = result.Usage ?? new Usage();

            return new ChatResult
            {
                Content = content,
                PromptTokens = usage.PromptTokens,
                CompletionTokens = usage.CompletionTokens,
                Duration = duration
            };
        }
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
        public Usage? Usage { get; set; }
    }

    private sealed class Choice
    {
        public ApiMessage? Message { get; set; }
    }

    private sealed class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
