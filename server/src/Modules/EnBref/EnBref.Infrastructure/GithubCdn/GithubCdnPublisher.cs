namespace EnBref.Infrastructure.GithubCdn;

public class GithubCdnPublisher : IObjectStorageWriter<Recap>
{
    private readonly HttpClient _client;
    private const string RelativeUri = "repos/yterraillon/yterraillon.github.io/contents/cdn/en-bref/data/latest-recap.json";
    
    public GithubCdnPublisher(HttpClient client, Settings settings)
    {
        _client = client;
        _client.AddBearerTokenHeader(settings.GithubToken ?? "");
    }
    
    public async Task<Uri> StoreObjectAsync(Recap content)
    {
        var sha = await GetFileSha();
        var base64EncodedContent = GetBase64EncodedRecap(content);
        var payload = BuildApiPayload(base64EncodedContent, sha);
        
        var response = await _client.PutRequest<GithubApiPayload, GithubApiResponse>(RelativeUri, payload);
        return new Uri(response?.Commit.Url ?? string.Empty);
    }

    private async Task<string> GetFileSha()
    {
        var response = await _client.GetRequest<GithubResponse>(RelativeUri);
        return response?.Sha ?? string.Empty;
    }
    
    private static string GetBase64EncodedRecap(Recap recap)
    {
        var content = JsonSerializer.Serialize(recap, JsonSerializerOptionsFactory.CreateCamelCaseAndWriteIntendedOptions());
        var bytes = Encoding.UTF8.GetBytes(content);
        return Convert.ToBase64String(bytes);
    }

    private static GithubApiPayload BuildApiPayload(string base64EncodedContent, string sha) =>
        new()
        {
            Content = base64EncodedContent,
            Sha = sha,
            Message = $"{DateTime.Now:dd-MM-yyyy} - Updated latest Recap"
        };
}