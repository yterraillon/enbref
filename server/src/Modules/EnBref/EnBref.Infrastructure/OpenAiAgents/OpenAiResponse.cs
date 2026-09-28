namespace EnBref.Infrastructure.OpenAiAgents;

public class OpenAiResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<Choice> Choices { get; set; } = [];
    public TokenUsage Usage { get; set; } = new TokenUsage();
    public string SystemFingerprint { get; set; } = string.Empty;
 
    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; } = new Message();
        public object? Logprobs { get; set; }
        public string FinishReason { get; set; } = string.Empty;
    }
 
    public class Message
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public object Refusal { get; set; } = string.Empty;
    }
 
    public class TokenUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}