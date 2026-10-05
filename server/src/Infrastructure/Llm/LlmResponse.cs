namespace EnBref.Infrastructure.Llm;

public enum LlmStatus
{
    /// <summary>Réponse complète.</summary>
    Completed,

    /// <summary>Le modèle a refusé de répondre.</summary>
    Refused,

    /// <summary>Réponse coupée par la limite de tokens : son contenu est incomplet.</summary>
    Truncated,

    /// <summary>LLM injoignable ou surchargé, après les tentatives du SDK.</summary>
    Unavailable,

    /// <summary>Requête refusée par l'API : clé, modèle ou paramètres invalides.</summary>
    Rejected,
}

public sealed record LlmResponse(LlmStatus Status, string Content, string? Error = null);
