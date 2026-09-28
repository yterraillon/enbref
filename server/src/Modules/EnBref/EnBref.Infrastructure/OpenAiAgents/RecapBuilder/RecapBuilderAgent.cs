using Application.AiAgents;
using Infrastructure.HttpClients;

namespace EnBref.Infrastructure.OpenAiAgents.RecapBuilder;

public class RecapBuilderAgent : IAiAgent<IEnumerable<string>, string>
{
    public string SystemPrompt { get; } 
    public string UserPrompt { get; }
    
    private readonly HttpClient _client;
    
    public RecapBuilderAgent(HttpClient client, Settings settings)
    {
        _client = client;
        _client.AddBearerTokenHeader(settings.OpenAiApiKey ?? "");
        SystemPrompt = string.Empty;
        UserPrompt = RecapBuilderPrompts.UserPrompt;
    }
    
    public async Task<string> Handle(IEnumerable<string> argument)
    {
        var formattedTitlesForPrompt = FormatTitlesForPrompt(argument);
        var prompt = UserPrompt + formattedTitlesForPrompt;
        
        var request = BuildOpenAiRequest(prompt);
        var result = await _client.PostRequest<OpenAiRequest, OpenAiResponse>(_client.BaseAddress!.ToString(), request);
        
        return result?.Choices[0].Message.Content ?? string.Empty;
    }

    private static string FormatTitlesForPrompt(IEnumerable<string> titles) => 
        titles.Aggregate(string.Empty, (current, title) => current + $"\n- {title}");

    private static OpenAiRequest BuildOpenAiRequest(string prompt)
    {
        var request = new OpenAiRequest
        {
            Messages =
            [
                new OpenAiRequest.Message
                {
                    Role = "user",
                    Content = prompt
                }
            ]
        };
        return request;
    }
}