namespace EnBref.Infrastructure.Publication;

/// <summary>Jeton lu dans <c>GithubToken</c> : user secrets en développement, variable d'environnement en production.</summary>
public sealed class GithubOptions
{
    public string Token { get; set; } = "";

    public string Repository { get; set; } = "yterraillon/yterraillon.github.io";

    public string Directory { get; set; } = "cdn/en-bref/data";

    public string Branch { get; set; } = "main";
}
