using Application;
using Application.AiAgents;
using Application.ObjectStorage;
using EnBref.Application.Contracts;
using EnBref.Application.Models;

namespace EnBref.Application.Features;

public static class GenerateDailyRecap
{
    public class Handler(IRssReader rssReader,
        IAiAgent<IEnumerable<string>, string> recapBuilderAgent,
        IAiAgent<string, Recap> recapFormatterAgent,
        IObjectStorageWriter<Recap> objectStorageWriter,
        IRecapSectionMetricRepository recapSectionMetricRepository,
        INotificationService notificationService, ILogger<Handler> logger)
        : IRequestHandler<Request, Response>
    {
        private const string Url20Minutes = "https://www.20minutes.fr/feeds/rss-une.xml";
        private const string UrlFigaro = "https://www.lefigaro.fr/rss/figaro_actualites.xml";

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            try
            {
                var titles = GetNewsTitles();
                var recapAsText = await recapBuilderAgent.Handle(titles);
                var recap = await recapFormatterAgent.Handle(recapAsText);
                recap.SetCreatedAt();
                await objectStorageWriter.StoreObjectAsync(recap);

                StoreRecapSectionMetrics(recap);

                return new Response(true, DateOnly.FromDateTime(DateTime.Now));
            }
            catch (Exception ex)
            {
                const string message = "[EnBref] An error occurred while generating the daily recap";
                await notificationService.SendNotification(message);
                logger.LogError("{Timestamp} {Message} - See exception : {ExceptionMessage}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message, ex.Message);
                return new Response(false, DateOnly.FromDateTime(DateTime.Now));
            }
        }

        private void StoreRecapSectionMetrics(Recap recap)
        {
            var sectionTitles = recap.Sections.Select(s => s.Title);
            var recapSectionMetric = RecapSectionMetric.Create(sectionTitles);
            recapSectionMetricRepository.Add(recapSectionMetric);
            logger.LogInformation("{Timestamp} Stored recap metrics with {Count} sections",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), recap.Sections.Count);
        }

        private List<string> GetNewsTitles()
        {
            var titles = new List<string>();
            
            var all20MinTitles = rssReader.GetRssFeedTitles(Url20Minutes);
            var allFigaroTitles = rssReader.GetRssFeedTitles(UrlFigaro);
            
            titles.AddRange(all20MinTitles);
            titles.AddRange(allFigaroTitles);

            return titles;
        }
    }

    public class Request : IRequest<Response>;

    public record Response(bool IsSuccess, DateOnly Date);
}