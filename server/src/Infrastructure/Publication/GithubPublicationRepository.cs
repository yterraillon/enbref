using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnBref.Infrastructure.Publication;

/// <summary>Publie un artefact sur GitHub Pages par l'API Contents de GitHub (ADR-001).</summary>
public sealed class GithubPublicationRepository(
    HttpClient httpClient,
    IOptions<GithubOptions> options,
    ILogger<GithubPublicationRepository> logger) : IPublicationRepository
{
    public async Task<PublicationResult> PublishAsync(string artifact, string content, string message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Token))
        {
            return Fail(artifact, "Jeton GitHub absent : renseigner GithubToken.");
        }

        var uri = $"repos/{settings.Repository}/contents/{settings.Directory}/{artifact}";
        try
        {
            var sha = await GetShaAsync(uri, settings, cancellationToken);
            var payload = new ContentPayload(message, Convert.ToBase64String(Encoding.UTF8.GetBytes(content)), sha, settings.Branch);

            using var request = Request(HttpMethod.Put, uri, settings);
            request.Content = JsonContent.Create(payload);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Fail(artifact, await DescribeAsync(response, cancellationToken));
            }

            var body = await response.Content.ReadFromJsonAsync<ContentResponse>(cancellationToken);
            var commitUrl = body?.Commit?.HtmlUrl is { } url ? new Uri(url) : null;
            logger.LogInformation("Publication de {Artifact} : {CommitUrl}", artifact, commitUrl);
            return new PublicationResult(commitUrl, Error: null);
        }
        catch (HttpRequestException exception)
        {
            return Fail(artifact, exception.Message);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return Fail(artifact, $"Timeout : {exception.Message}");
        }
    }

    // Le dépôt est public : la lecture se passe du jeton, donc fonctionne en local où il est vide.
    public async Task<ArtifactReadResult> ReadAsync(string artifact, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var uri = $"repos/{settings.Repository}/contents/{settings.Directory}/{artifact}?ref={settings.Branch}";
        try
        {
            using var request = Request(HttpMethod.Get, uri, settings);
            // Contenu brut plutôt que l'enveloppe JSON en base64.
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.raw+json"));
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ArtifactReadResult(Content: null, $"Artefact absent : {artifact}.", IsMissing: true);
            }

            if (!response.IsSuccessStatusCode)
            {
                return new ArtifactReadResult(Content: null, await DescribeAsync(response, cancellationToken));
            }

            return new ArtifactReadResult(await response.Content.ReadAsStringAsync(cancellationToken), Error: null);
        }
        catch (HttpRequestException exception)
        {
            return new ArtifactReadResult(Content: null, exception.Message);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return new ArtifactReadResult(Content: null, $"Timeout : {exception.Message}");
        }
    }

    // Sans sha, GitHub crée le fichier ; avec, il le remplace.
    private async Task<string?> GetShaAsync(string uri, GithubOptions settings, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Get, $"{uri}?ref={settings.Branch}", settings);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await DescribeAsync(response, cancellationToken));
        }

        var file = await response.Content.ReadFromJsonAsync<FileResponse>(cancellationToken);
        return file?.Sha;
    }

    private static HttpRequestMessage Request(HttpMethod method, string uri, GithubOptions settings)
    {
        var request = new HttpRequestMessage(method, uri);
        if (!string.IsNullOrWhiteSpace(settings.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
        }

        return request;
    }

    private static async Task<string> DescribeAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        $"GitHub {(int)response.StatusCode} {response.ReasonPhrase} : {await response.Content.ReadAsStringAsync(cancellationToken)}";

    private PublicationResult Fail(string artifact, string error)
    {
        logger.LogError("Publication de {Artifact} en échec : {Error}", artifact, error);
        return new PublicationResult(CommitUrl: null, error);
    }

    private sealed record ContentPayload(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("sha"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Sha,
        [property: JsonPropertyName("branch")] string Branch);

    private sealed record FileResponse([property: JsonPropertyName("sha")] string? Sha);

    private sealed record ContentResponse([property: JsonPropertyName("commit")] CommitResponse? Commit);

    private sealed record CommitResponse([property: JsonPropertyName("html_url")] string? HtmlUrl);
}
