using System.Text.Json.Serialization;

namespace EnBref.Infrastructure.OpenAiAgents;

public class OpenAiRequest
{
    private static class OpenAiDefaultSettings
    {
        public const string DefaultRole = "user";
        public const string DefaultModel = "gpt-4o";
        public const double DefaultTemperature = 0.3; // Une température basse favorise des réponses factuelles et cohérentes, tandis qu'une température élevée favorise la créativité.
        public const double DefaultFrequencyPenalty = 0.2; // Réduit les répétitions tout en maintenant une certaine fluidité dans la rédaction. 
        public const int DefaultMaxCompletionTokens = 1000; // Suggéré : 300 à 500 - Permet de générer des réponses concises mais complètes pour des résumés d'actualité (environ 2 à 4 paragraphes). Si les résumés doivent être plus détaillés, on peut augmenter cette valeur (par exemple, 750).
    }
    
    public string Model { get; set; } = OpenAiDefaultSettings.DefaultModel;

    [JsonPropertyName("messages")] 
    public List<Message> Messages { get; set; } = [];
    public double Temperature { get; set; } = OpenAiDefaultSettings.DefaultTemperature;
    
    [JsonPropertyName("frequency_penalty")] 
    public double FrequencyPenalty { get; set; } = OpenAiDefaultSettings.DefaultFrequencyPenalty;
    
    [JsonPropertyName("max_completion_tokens")]
    public int MaxCompletionTokens { get; set; } = OpenAiDefaultSettings.DefaultMaxCompletionTokens;

 
    public class Message
    {
        public string Role { get; set; } = OpenAiDefaultSettings.DefaultRole;
        public string Content { get; set; } = string.Empty;
    }
}