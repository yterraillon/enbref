using Application;
using Infrastructure.HttpClients;

namespace Infrastructure.Notifications;

public class Ntfy : INotificationService
{
    private readonly HttpClient _client;

    public Ntfy(HttpClient client, Settings settings)
    {
        _client = client;
        _client.AddBearerTokenHeader(settings.NtfyToken ?? string.Empty);
    }
    
    public async Task<bool> SendNotification(string message)
    {
        var result = await _client.PostRequest<string, NtfyResponse>(_client.BaseAddress!.ToString(), message);
        return result is not null && result.Id != string.Empty;
    }
}

public class NtfyResponse
{
    public string Id { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Time { get; set; }
    public long Expires { get; set; }
}