using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnBref.Infrastructure.Alerting;

public sealed class NtfyAlertSender(HttpClient httpClient, IOptions<NtfyOptions> options, ILogger<NtfyAlertSender> logger) : IAlertSender
{
    public async Task<AlertResult> SendAsync(Alert alert, CancellationToken cancellationToken)
    {
        var ntfy = options.Value;
        if (string.IsNullOrWhiteSpace(ntfy.Topic))
        {
            logger.LogWarning("Alerte non envoyée, aucun topic ntfy configuré : {Title} — {Message}", alert.Title, alert.Message);
            return new AlertResult("Aucun topic ntfy configuré.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ntfy.Url.TrimEnd('/')}/{Uri.EscapeDataString(ntfy.Topic)}")
        {
            Content = new StringContent(alert.Message, Encoding.UTF8, "text/plain"),
        };
        // Un en-tête HTTP reste en ASCII : l'encodage RFC 2047, que ntfy décode, garde les accents du titre.
        request.Headers.Add("Title", $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(alert.Title))}?=");
        request.Headers.Add("Priority", "high");
        request.Headers.Add("Tags", "warning");
        if (!string.IsNullOrWhiteSpace(ntfy.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ntfy.Token);
        }

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Alerte envoyée sur ntfy : {Title}.", alert.Title);
                return new AlertResult(Error: null);
            }

            var error = $"ntfy a répondu {(int)response.StatusCode} {response.ReasonPhrase}.";
            logger.LogError("Alerte {Title} non envoyée : {Error}", alert.Title, error);
            return new AlertResult(error);
        }
        catch (Exception exception) when (exception is HttpRequestException
                                              || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogError(exception, "Alerte {Title} non envoyée : ntfy injoignable.", alert.Title);
            return new AlertResult($"ntfy injoignable : {exception.Message}");
        }
    }
}
