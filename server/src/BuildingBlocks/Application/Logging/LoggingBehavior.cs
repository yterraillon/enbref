using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Logging;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation("{Timestamp} Handling {Name}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), typeof(TRequest).FullName);
        var response = await next();
        logger.LogInformation("{Timestamp} Handled {Name}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), typeof(TResponse).FullName);

        return response;
    }
}