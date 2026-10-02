using System.Net;
using System.Text;
using EnBref.Infrastructure.Collection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Infrastructure.Tests.Collection;

public class RssFeedReaderTests
{
    private static readonly Feed Feed = new("Source", new Uri("https://source.test/rss"));

    private static Task<FeedResult> ReadAsync(Func<HttpResponseMessage> respond)
    {
        var httpClient = new HttpClient(new StubHandler(respond));
        return new RssFeedReader(httpClient, NullLogger<RssFeedReader>.Instance).ReadAsync(Feed, CancellationToken.None);
    }

    private static HttpResponseMessage Content(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/xml") };

    private static string Rss(params string[] titles) =>
        $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0"><channel><title>Flux</title><link>https://source.test</link><description>d</description>
        {string.Concat(titles.Select(title => $"<item><title>{title}</title></item>"))}
        </channel></rss>
        """;

    [Test]
    public async Task Rss_headlines_are_trimmed_blanks_dropped_and_duplicates_kept()
    {
        var result = await ReadAsync(() => Content(Rss("  Premier titre  ", "", "   ", "Doublon", "Doublon")));

        await Assert.That(result.Status).IsEqualTo(FeedStatus.Available);
        await Assert.That(result.Headlines).IsEquivalentTo(["Premier titre", "Doublon", "Doublon"]);
    }

    [Test]
    public async Task Atom_feed_is_available()
    {
        const string atom = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom"><title>Flux</title><id>urn:flux</id><updated>2026-10-02T00:00:00Z</updated>
            <entry><title>Titre Atom</title><id>urn:1</id><updated>2026-10-02T00:00:00Z</updated></entry></feed>
            """;

        var result = await ReadAsync(() => Content(atom));

        await Assert.That(result.Status).IsEqualTo(FeedStatus.Available);
        await Assert.That(result.Headlines).IsEquivalentTo(["Titre Atom"]);
    }

    [Test]
    public async Task Server_error_is_unreachable()
    {
        var result = await ReadAsync(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await Assert.That(result.Status).IsEqualTo(FeedStatus.Unreachable);
    }

    [Test]
    public async Task Timeout_is_unreachable()
    {
        var result = await ReadAsync(() => throw new TaskCanceledException("timeout"));

        await Assert.That(result.Status).IsEqualTo(FeedStatus.Unreachable);
    }

    [Test]
    public async Task Non_xml_content_is_invalid()
    {
        var result = await ReadAsync(() => Content("<html>pas un flux"));

        await Assert.That(result.Status).IsEqualTo(FeedStatus.Invalid);
    }

    [Test]
    public async Task Feed_without_items_is_empty()
    {
        var result = await ReadAsync(() => Content(Rss()));

        await Assert.That(result.Status).IsEqualTo(FeedStatus.Empty);
    }

    private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond());
    }
}
