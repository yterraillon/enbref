using Application.AiAgents;
using Infrastructure.HttpClients;

namespace EnBref.Infrastructure.OpenAiAgents.RecapFormatter;

public class RecapFormatterAgent : IAiAgent<string, Recap>
{
    public string SystemPrompt { get; }
    public string UserPrompt { get; }
    
    private readonly HttpClient _client;

    public RecapFormatterAgent(HttpClient client, Settings settings)
    {
        _client = client;
        _client.AddBearerTokenHeader(settings.OpenAiApiKey ?? "");
        SystemPrompt = RecapFormatterPrompts.SystemPrompt;
        UserPrompt = RecapFormatterPrompts.UserPrompt;
    }
    
    public async Task<Recap> Handle(string argument)
    {
        var request = BuildOpenAiRequest(argument);
        var result = await _client.PostRequest<OpenAiRequest, OpenAiResponse>(_client.BaseAddress!.ToString(), request);
        var content = result?.Choices[0].Message.Content;
        var contentWithoutJsonQuotations = RemoveJsonQuotations(content);
        
        if (string.IsNullOrEmpty(contentWithoutJsonQuotations))
            return Recap.EmptyRecap;
        
        var recap = JsonSerializer.Deserialize<Recap>(contentWithoutJsonQuotations) ?? Recap.EmptyRecap;
        recap.Title = BuildRecapTitle();
        return recap;
    }
    
    private static OpenAiRequest BuildOpenAiRequest(string recap)
    {
        var request = new OpenAiRequest
        {
            Messages =
            [
                new OpenAiRequest.Message
                {
                    Role = "system",
                    Content = RecapFormatterPrompts.SystemPrompt
                },
                new OpenAiRequest.Message
                {
                    Role = "user",
                    Content = RecapFormatterPrompts.UserPrompt + recap
                }
            ],
            Temperature = 0.2,
            MaxCompletionTokens = 1000,
            FrequencyPenalty = 0.0
        };
        return request;
    }

    private static string BuildRecapTitle()
    {
        var today = DateTime.Now.ToString("dd MMMM", new System.Globalization.CultureInfo("fr-FR"));
        return $"Récap du {today}";
    }

    private static string? RemoveJsonQuotations(string? content) =>
        content?
            .Replace("```json", string.Empty)
            .Replace("```", string.Empty)
            .Trim();
}