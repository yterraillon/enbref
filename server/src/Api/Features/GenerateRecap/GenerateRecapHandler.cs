namespace EnBref.Api.Features.GenerateRecap;

public sealed class GenerateRecapHandler
{
    // Étape 2 : collecte, génération, puis publication — jamais pour un RecapKind.Test.
    public Task HandleAsync(GenerateRecapCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
