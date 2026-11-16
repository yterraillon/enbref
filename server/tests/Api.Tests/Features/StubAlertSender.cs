using EnBref.Infrastructure.Alerting;

namespace EnBref.Api.Tests.Features;

/// <summary>Envoi d'alertes qui garde les alertes reçues.</summary>
internal sealed class StubAlertSender : IAlertSender
{
    public List<Alert> Alerts { get; } = [];

    public Task<AlertResult> SendAsync(Alert alert, CancellationToken cancellationToken)
    {
        Alerts.Add(alert);
        return Task.FromResult(new AlertResult(Error: null));
    }
}
