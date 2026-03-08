namespace OpenRouterChat.Models;

public class ChatResult
{
    public string Content { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public TimeSpan Duration { get; set; }
}