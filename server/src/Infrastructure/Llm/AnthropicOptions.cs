namespace EnBref.Infrastructure.Llm;

/// <summary>Clé lue dans le Secret Manager en développement, dans <c>Anthropic__ApiKey</c> en production.</summary>
public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; init; } = "";

    public string Model { get; init; } = "";
}
