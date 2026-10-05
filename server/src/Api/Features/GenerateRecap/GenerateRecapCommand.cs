using EnBref.Api.Shared;

namespace EnBref.Api.Features.GenerateRecap;

/// <param name="Type">Type de récap à générer.</param>
/// <param name="Publish">Faux : la génération s'arrête avant la publication.</param>
public sealed record GenerateRecapCommand(RecapType Type, bool Publish);
