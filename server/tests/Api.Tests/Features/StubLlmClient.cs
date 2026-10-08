using EnBref.Infrastructure.Llm;

namespace EnBref.Api.Tests.Features;

/// <summary>Client LLM qui renvoie une réponse choisie, compte ses appels et garde la dernière requête.</summary>
internal sealed class StubLlmClient(LlmResponse response) : ILlmClient
{
    public int Calls { get; private set; }

    public LlmRequest? LastRequest { get; private set; }

    public static StubLlmClient Completed(string content) => new(new LlmResponse(LlmStatus.Completed, content));

    public Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        Calls++;
        LastRequest = request;
        return Task.FromResult(response);
    }
}
