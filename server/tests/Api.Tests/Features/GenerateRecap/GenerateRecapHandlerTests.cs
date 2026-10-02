using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.Features.GenerateRecap;

// Étape 2 : collecte, génération, publication, et récap de test jamais publié.
public class GenerateRecapHandlerTests
{
    [Test]
    [Arguments(RecapType.Daily, true)]
    [Arguments(RecapType.Demo, true)]
    [Arguments(RecapType.Test, false)]
    public async Task Handle_completes(RecapType type, bool publish)
    {
        var handler = new GenerateRecapHandler();

        var handling = handler.HandleAsync(new GenerateRecapCommand(type, publish), CancellationToken.None);

        await handling;
        await Assert.That(handling.IsCompletedSuccessfully).IsTrue();
    }
}
