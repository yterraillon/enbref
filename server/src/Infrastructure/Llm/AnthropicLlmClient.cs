using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Options;

namespace EnBref.Infrastructure.Llm;

public sealed class AnthropicLlmClient(AnthropicClient client, IOptions<AnthropicOptions> options) : ILlmClient
{
    public async Task<string> SendAsync(string prompt, CancellationToken cancellationToken)
    {
        // Sans ce garde-fou, le SDK irait chercher ANTHROPIC_API_KEY en silence.
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            throw new InvalidOperationException("Clé Anthropic absente : renseigner Anthropic:ApiKey.");
        }

        MessageCreateParams parameters = new()
        {
            MaxTokens = 1024,
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = prompt,
                },
            ],
            Model = options.Value.Model,
        };

        var message = await client.Messages.Create(parameters, cancellationToken);

        return string.Concat(message.Content.Select(block => block.Value).OfType<TextBlock>().Select(text => text.Text));
    }
}
