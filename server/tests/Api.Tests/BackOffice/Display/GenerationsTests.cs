using EnBref.Api.BackOffice.Display;
using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.BackOffice.Display;

public class GenerationsTests
{
    private static readonly CollectionResult Collection = new([]);

    [Test]
    [Arguments(RecapType.Daily, true)]
    [Arguments(RecapType.Demo, true)]
    [Arguments(RecapType.Test, false)]
    public async Task Only_the_test_recap_skips_confirmation(RecapType type, bool requiresConfirmation)
    {
        await Assert.That(Generations.RequiresConfirmation(type)).IsEqualTo(requiresConfirmation);
    }

    [Test]
    public async Task Successful_generation_says_it_was_published()
    {
        var feedback = Generations.Feedback(RecapType.Demo, new GenerateRecapResult(Collection));

        await Assert.That(feedback).IsEqualTo("Récap de démo généré et publié.");
    }

    [Test]
    public async Task Failed_generation_names_its_step()
    {
        var result = new GenerateRecapResult(Collection, Failure: new GenerateRecapFailure(GenerationStep.Publication, "502"));

        await Assert.That(Generations.Feedback(RecapType.Daily, result)).IsEqualTo("Erreur lors de la publication : 502");
    }
}
