namespace EnBref.Infrastructure.Alerting;

/// <summary>Envoi d'une alerte. Ne lève jamais : un échec d'envoi est rendu dans <see cref="AlertResult"/>.</summary>
public interface IAlertSender
{
    Task<AlertResult> SendAsync(Alert alert, CancellationToken cancellationToken);
}
