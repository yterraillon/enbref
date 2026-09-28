using Infrastructure.HttpClients;

namespace EnBref.Infrastructure.GithubCdn;

public class GithubCdnReader(HttpClient client) : IObjectStorageReader<Recap>
{
    public async Task<Recap> GetObjectContentAsync(string objectName)
    {
        var path = "/cdn/en-bref/data/" + objectName;
        
        var result = await client.GetRequest<Recap>(path);
        return result ?? Recap.EmptyRecap;
    }
}