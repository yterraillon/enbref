namespace EnBref.Infrastructure.Llm;

public interface ILlmClient
{
    /// <summary>Constate la réponse du LLM sans jamais lever, hors annulation.</summary>
    Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken);
}
