using EnBref.Infrastructure.Llm;

namespace EnBref.Api.Tests.Features;

/// <summary>Client LLM qui renvoie une réponse fixe et compte ses appels.</summary>
internal sealed class StubLlmClient(string response = "réponse") : ILlmClient
{
    public int Calls { get; private set; }

    public Task<string> SendAsync(CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(response);
    }
}
