using System.ServiceModel.Syndication;
using System.Xml;

namespace Infrastructure.RssReader;

public class RssReaderService : IRssReader
{
    public SyndicationItem? GetLatestFeedItem(string url)
    {
        var feed = LoadFeed(url);
        return feed.Items.First();
    }

    public IEnumerable<SyndicationItem> GetAllFeedItems(string url)
    {
        var feed = LoadFeed(url);
        return feed.Items;
    }
    
    private static SyndicationFeed LoadFeed(string url)
    {
        using var reader = XmlReader.Create(url);
        return SyndicationFeed.Load(reader);
    }
}