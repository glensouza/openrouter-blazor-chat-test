namespace OpenRouterChat.Models;

public class ComparisonResult
{
    public string ModelName { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public double TokensPerSecond { get; set; }

}
