using System.Net;
using System.Text;
using System.Text.Json;
using EnBref.Infrastructure.Publication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnBref.Infrastructure.Tests.Publication;

public class GithubPublicationRepositoryTests
{
    private const string CommitResponse = """{ "commit": { "html_url": "https://github.test/commit/abc" } }""";

    private static (GithubPublicationRepository Repository, StubHandler Handler) Repository(string token, params HttpResponseMessage[] responses)
    {
        var handler = new StubHandler(responses);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.test/") };
        var repository = new GithubPublicationRepository(httpClient, Options.Create(new GithubOptions { Token = token }), NullLogger<GithubPublicationRepository>.Instance);
        return (repository, handler);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    [Test]
    public async Task Missing_token_calls_nothing()
    {
        var (repository, handler) = Repository("");

        var result = await repository.PublishAsync("test.json", "{}", "message", CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(handler.Requests).IsEmpty();
    }

    [Test]
    public async Task New_artifact_is_created_without_sha()
    {
        var (repository, handler) = Repository("jeton",
            new HttpResponseMessage(HttpStatusCode.NotFound),
            Json(HttpStatusCode.Created, CommitResponse));

        var result = await repository.PublishAsync("test.json", """{"date":"2026-10-08"}""", "message", CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.CommitUrl).IsEqualTo(new Uri("https://github.test/commit/abc"));

        var put = handler.Requests[1];
        await Assert.That(put.Method).IsEqualTo(HttpMethod.Put);
        await Assert.That(put.Uri).IsEqualTo("https://api.github.test/repos/yterraillon/yterraillon.github.io/contents/cdn/en-bref/data/test.json");

        using var payload = JsonDocument.Parse(put.Body!);
        await Assert.That(payload.RootElement.TryGetProperty("sha", out _)).IsFalse();
        await Assert.That(Encoding.UTF8.GetString(Convert.FromBase64String(payload.RootElement.GetProperty("content").GetString()!)))
            .IsEqualTo("""{"date":"2026-10-08"}""");
    }

    [Test]
    public async Task Existing_artifact_is_replaced_with_its_sha()
    {
        var (repository, handler) = Repository("jeton",
            Json(HttpStatusCode.OK, """{ "sha": "abc123" }"""),
            Json(HttpStatusCode.OK, CommitResponse));

        await repository.PublishAsync("latest.json", "{}", "message", CancellationToken.None);

        using var payload = JsonDocument.Parse(handler.Requests[1].Body!);
        await Assert.That(payload.RootElement.GetProperty("sha").GetString()).IsEqualTo("abc123");
        await Assert.That(handler.Requests[0].Authorization).IsEqualTo("Bearer jeton");
    }

    [Test]
    public async Task Rejected_publication_is_an_error()
    {
        var (repository, _) = Repository("jeton",
            new HttpResponseMessage(HttpStatusCode.NotFound),
            Json(HttpStatusCode.UnprocessableEntity, """{ "message": "Invalid request." }"""));

        var result = await repository.PublishAsync("test.json", "{}", "message", CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Error).Contains("422");
    }

    [Test]
    public async Task Artifact_is_read_raw_without_token()
    {
        var (repository, handler) = Repository("", Json(HttpStatusCode.OK, """{"date":"2026-10-09"}"""));

        var result = await repository.ReadAsync("demo.json", CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Content).IsEqualTo("""{"date":"2026-10-09"}""");

        var get = handler.Requests[0];
        await Assert.That(get.Method).IsEqualTo(HttpMethod.Get);
        await Assert.That(get.Uri).IsEqualTo("https://api.github.test/repos/yterraillon/yterraillon.github.io/contents/cdn/en-bref/data/demo.json");
        await Assert.That(get.Accept).Contains("application/vnd.github.raw+json");
        await Assert.That(get.Authorization).IsNull();
    }

    [Test]
    public async Task Artifact_is_read_with_token_when_set()
    {
        var (repository, handler) = Repository("jeton", Json(HttpStatusCode.OK, "{}"));

        await repository.ReadAsync("latest.json", CancellationToken.None);

        await Assert.That(handler.Requests[0].Authorization).IsEqualTo("Bearer jeton");
    }

    [Test]
    [Arguments(HttpStatusCode.NotFound, "absent", true)]
    [Arguments(HttpStatusCode.InternalServerError, "500", false)]
    public async Task Unreadable_artifact_is_an_error(HttpStatusCode status, string expected, bool isMissing)
    {
        var (repository, _) = Repository("", Json(status, "{}"));

        var result = await repository.ReadAsync("demo.json", CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Content).IsNull();
        await Assert.That(result.Error).Contains(expected);
        await Assert.That(result.IsMissing).IsEqualTo(isMissing);
    }

    private sealed class StubHandler(HttpResponseMessage[] responses) : HttpMessageHandler
    {
        public List<(HttpMethod Method, string Uri, string? Body, string? Authorization, string Accept)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.Method, request.RequestUri!.GetLeftPart(UriPartial.Path), body, request.Headers.Authorization?.ToString(), request.Headers.Accept.ToString()));
            return responses[Requests.Count - 1];
        }
    }
}
