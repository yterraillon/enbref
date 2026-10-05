namespace EnBref.Infrastructure.Llm;

public interface ILlmClient
{
    /// <summary>Envoie un prompt utilisateur et renvoie le texte de la réponse.</summary>
    Task<string> SendAsync(string prompt, CancellationToken cancellationToken);
}
