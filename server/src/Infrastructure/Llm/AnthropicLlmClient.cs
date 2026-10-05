using System.Text.Json;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnBref.Infrastructure.Llm;

public sealed class AnthropicLlmClient(
    AnthropicClient client,
    IOptions<AnthropicOptions> options,
    ILogger<AnthropicLlmClient> logger) : ILlmClient
{
    // Opus 5.5 pense toujours : la réflexion se décompte de ce plafond.
    private const int MaxTokens = 16000;

    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        // Sans ce garde-fou, le SDK irait chercher ANTHROPIC_API_KEY en silence.
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            return new LlmResponse(LlmStatus.Rejected, "", "Clé Anthropic absente : renseigner Anthropic:ApiKey.");
        }

        MessageCreateParams parameters = new()
        {
            MaxTokens = MaxTokens,
            System = request.Prompt,
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = request.Context,
                },
            ],
            Model = options.Value.Model,
            OutputConfig = request.OutputSchema is null
                ? null
                : new OutputConfig
                {
                    Format = new JsonOutputFormat
                    {
                        Schema = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.OutputSchema)!,
                    },
                },
        };

        // Le SDK a déjà retenté les 429, 5xx et erreurs réseau avant de lever.
        try
        {
            var message = await client.Messages.Create(parameters, cancellationToken);
            var content = string.Concat(message.Content.Select(block => block.Value).OfType<TextBlock>().Select(text => text.Text));

            if (message.StopReason == StopReason.Refusal)
            {
                var details = message.StopDetails;
                return new LlmResponse(LlmStatus.Refused, content, $"Refus du modèle : {details?.Category} {details?.Explanation}".Trim());
            }

            return message.StopReason == StopReason.MaxTokens
                ? new LlmResponse(LlmStatus.Truncated, content, $"Réponse coupée à {MaxTokens} tokens.")
                : new LlmResponse(LlmStatus.Completed, content);
        }
        catch (AnthropicRateLimitException exception)
        {
            return Fail(LlmStatus.Unavailable, exception);
        }
        catch (Anthropic5xxException exception)
        {
            return Fail(LlmStatus.Unavailable, exception);
        }
        catch (AnthropicIOException exception)
        {
            return Fail(LlmStatus.Unavailable, exception);
        }
        catch (Anthropic4xxException exception)
        {
            return Fail(LlmStatus.Rejected, exception);
        }
    }

    private LlmResponse Fail(LlmStatus status, Exception exception)
    {
        logger.LogError(exception, "Appel Anthropic en échec : {Status}.", status);
        return new LlmResponse(status, "", exception.Message);
    }
}
