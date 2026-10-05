namespace EnBref.Infrastructure.Llm;

/// <param name="Prompt">Consignes données au modèle (system prompt).</param>
/// <param name="Context">Matière fournie au modèle (message utilisateur).</param>
/// <param name="OutputSchema">Schéma JSON imposé à la réponse ; null pour du texte libre.</param>
public sealed record LlmRequest(string Prompt, string Context, string? OutputSchema = null);
