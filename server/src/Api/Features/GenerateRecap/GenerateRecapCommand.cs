using EnBref.Api.Shared;

namespace EnBref.Api.Features.GenerateRecap;

/// <param name="Type">Type de récap à générer.</param>
/// <param name="Publish">Ignoré pour un récap de test, qui n'est jamais publié.</param>
public sealed record GenerateRecapCommand(RecapType Type, bool Publish);
