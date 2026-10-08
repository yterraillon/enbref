namespace EnBref.Api.Features.GenerateRecap;

/// <summary>Ce que le rédacteur reçoit pour écrire un récap.</summary>
public sealed record GenerationContext(DateOnly Date, IReadOnlyList<string> Headlines);

public sealed record RecapWriterResult(Recap? Recap, string? Error)
{
    public bool IsSuccessful => Recap is not null;
}

/// <summary>Écrit un récap à partir des titres collectés : par le LLM, ou sans LLM pour le récap de test.</summary>
public interface IRecapWriter
{
    Task<RecapWriterResult> WriteAsync(GenerationContext context, CancellationToken cancellationToken);
}
