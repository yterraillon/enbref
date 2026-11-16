namespace EnBref.Api.Shared;

/// <summary>Répertoire des données du serveur : le volume <c>/data</c> en production.</summary>
public sealed class DataOptions
{
    public const string SectionName = "Data";

    public string Directory { get; set; } = "/data";
}
