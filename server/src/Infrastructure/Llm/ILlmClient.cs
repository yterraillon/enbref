namespace EnBref.Infrastructure.Llm;

public interface ILlmClient
{
    /// <summary>Interroge le LLM et renvoie le texte de sa réponse.</summary>
    Task<string> SendAsync(CancellationToken cancellationToken);
}
