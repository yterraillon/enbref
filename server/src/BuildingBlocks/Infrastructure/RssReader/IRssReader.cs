using System.ServiceModel.Syndication;

namespace Infrastructure.RssReader;

public interface IRssReader
{
    public SyndicationItem? GetLatestFeedItem(string url);
    
    public IEnumerable<SyndicationItem> GetAllFeedItems(string url);
}