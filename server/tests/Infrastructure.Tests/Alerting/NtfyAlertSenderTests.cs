using System.Net;
using System.Text;
using EnBref.Infrastructure.Alerting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnBref.Infrastructure.Tests.Alerting;

public class NtfyAlertSenderTests
{
    private static readonly Alert Alert = new("Récap du jour non publié", "Erreur lors de la publication.");

    private static (NtfyAlertSender Sender, StubHandler Handler) Sender(NtfyOptions options, Func<HttpResponseMessage> respond)
    {
        var handler = new StubHandler(respond);
        return (new NtfyAlertSender(new HttpClient(handler), Options.Create(options), NullLogger<NtfyAlertSender>.Instance), handler);
    }

    [Test]
    public async Task Alert_is_posted_on_the_topic()
    {
        var (sender, handler) = Sender(new NtfyOptions { Url = "https://ntfy.test/", Topic = "enbref", Token = "jeton" },
            () => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await sender.SendAsync(Alert, CancellationToken.None);

        await Assert.That(result.IsSent).IsTrue();
        var request = handler.Requests.Single();
        await Assert.That(request.Uri).IsEqualTo("https://ntfy.test/enbref");
        await Assert.That(request.Body).IsEqualTo("Erreur lors de la publication.");
        await Assert.That(request.Authorization).IsEqualTo("Bearer jeton");
        await Assert.That(request.Priority).IsEqualTo("high");
        await Assert.That(Decode(request.Title)).IsEqualTo("Récap du jour non publié");
    }

    [Test]
    public async Task Public_topic_is_posted_without_authorization()
    {
        var (sender, handler) = Sender(new NtfyOptions { Topic = "enbref" }, () => new HttpResponseMessage(HttpStatusCode.OK));

        await sender.SendAsync(Alert, CancellationToken.None);

        await Assert.That(handler.Requests.Single().Authorization).IsNull();
    }

    [Test]
    public async Task Missing_topic_calls_nothing()
    {
        var (sender, handler) = Sender(new NtfyOptions(), () => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await sender.SendAsync(Alert, CancellationToken.None);

        await Assert.That(result.IsSent).IsFalse();
        await Assert.That(handler.Requests).IsEmpty();
    }

    [Test]
    public async Task Error_status_is_reported()
    {
        var (sender, _) = Sender(new NtfyOptions { Topic = "enbref" }, () => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await sender.SendAsync(Alert, CancellationToken.None);

        await Assert.That(result.Error).Contains("500");
    }

    [Test]
    public async Task Unreachable_ntfy_is_reported()
    {
        var (sender, _) = Sender(new NtfyOptions { Topic = "enbref" }, () => throw new HttpRequestException("refusé"));

        var result = await sender.SendAsync(Alert, CancellationToken.None);

        await Assert.That(result.Error).Contains("injoignable");
    }

    private static string Decode(string? encodedWord) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(encodedWord!["=?UTF-8?B?".Length..^2]));

    private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(string Uri, string? Body, string? Authorization, string? Title, string? Priority)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.RequestUri!.ToString(), body, request.Headers.Authorization?.ToString(),
                Header(request, "Title"), Header(request, "Priority")));
            return respond();
        }

        private static string? Header(HttpRequestMessage request, string name) =>
            request.Headers.TryGetValues(name, out var values) ? values.Single() : null;
    }
}
