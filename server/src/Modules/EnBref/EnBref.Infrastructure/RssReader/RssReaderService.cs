using System.Xml;
using EnBref.Application.Contracts;
using System.ServiceModel.Syndication;

namespace EnBref.Infrastructure.RssReader;

public class RssReaderService : IRssReader
{
    public IEnumerable<string> GetRssFeedTitles(string url)
    {
        using var reader = XmlReader.Create(url);
        var feed = SyndicationFeed.Load(reader);
        reader.Close();

        var titles = feed.Items.Select(item => item.Title.Text).ToList();

        return titles;
    }
}