using Application.ObjectStorage;
using static EnBref.Application.Contracts.Constants;

namespace EnBref.Application.Features;

public static class DisplayLatestRecap
{
    public class Handler(IObjectStorageReader<Recap> azureStorageReader) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var latestRecap = await azureStorageReader.GetObjectContentAsync(LatestRecapFileName);
            return new Response(latestRecap);
        }
    }

    public class Request : IRequest<Response>;

    public record Response(Recap Recap);
}