using System.Xml;
using System.Xml.Linq;
using EnBref.Api.Features.PublishRecapFeed;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.Features.PublishRecapFeed;

public class RecapFeedTests
{
    private static readonly DateTimeOffset PublishedAt = new(2026, 11, 16, 16, 0, 0, TimeSpan.Zero);

    internal static Recap Recap(DateOnly date, string title = "Titre") => new(
        date,
        Enum.GetValues<Category>().ToDictionary(
            category => category,
            IReadOnlyList<Brief> (category) => [new Brief($"{title} {category}", "Une phrase.")]));

    private static XElement[] Items(string feed) => XDocument.Parse(feed).Root!.Element("channel")!.Elements("item").ToArray();

    private static string Guid(XElement item) => item.Element("guid")!.Value;

    [Test]
    public async Task Item_names_the_day_and_carries_a_stable_guid()
    {
        var item = RecapFeed.Item(Recap(new DateOnly(2026, 11, 16)), PublishedAt);

        await Assert.That(item.Element("title")!.Value).IsEqualTo("Récap du 16 novembre 2026");
        await Assert.That(Guid(item)).IsEqualTo("enbref-2026-11-16");
        await Assert.That(item.Element("guid")!.Attribute("isPermaLink")!.Value).IsEqualTo("false");
        await Assert.That(item.Element("pubDate")!.Value).IsEqualTo("Mon, 16 Nov 2026 16:00:00 GMT");
    }

    [Test]
    public async Task Item_lists_the_categories_in_glossary_order_with_their_labels()
    {
        var html = RecapFeed.Item(Recap(new DateOnly(2026, 11, 16)), PublishedAt).Element("description")!.Value;

        var positions = Enum.GetValues<Category>().Select(category => html.IndexOf($"<h2>{category.Label().Replace("&", "&amp;", StringComparison.Ordinal)}</h2>", StringComparison.Ordinal)).ToList();
        await Assert.That(positions).DoesNotContain(-1);
        await Assert.That(positions).IsInOrder();
    }

    [Test]
    public async Task Item_encodes_brief_text()
    {
        var recap = new Recap(new DateOnly(2026, 11, 16), new Dictionary<Category, IReadOnlyList<Brief>>
        {
            [Category.Economy] = [new Brief("<script> & ]]>", "a < b")],
        });

        var feed = RecapFeed.Merge(existing: null, RecapFeed.Item(recap, PublishedAt), PublishedAt);
        var html = Items(feed).Single().Element("description")!.Value;

        await Assert.That(html).Contains("&lt;script&gt; &amp; ]]&gt;");
        await Assert.That(html).Contains("a &lt; b");
        await Assert.That(html).DoesNotContain("<script>");
    }

    [Test]
    public async Task Merge_without_a_feed_starts_one()
    {
        var feed = RecapFeed.Merge(existing: null, RecapFeed.Item(Recap(new DateOnly(2026, 11, 16)), PublishedAt), PublishedAt);

        var document = XDocument.Parse(feed);
        await Assert.That(feed).StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        await Assert.That(document.Root!.Attribute("version")!.Value).IsEqualTo("2.0");
        await Assert.That(document.Root.Element("channel")!.Element("title")!.Value).IsEqualTo("EnBref");
        await Assert.That(Items(feed).Select(Guid)).IsEquivalentTo(["enbref-2026-11-16"]);
    }

    [Test]
    public async Task Merge_puts_the_new_item_first()
    {
        var yesterday = RecapFeed.Merge(null, RecapFeed.Item(Recap(new DateOnly(2026, 11, 15)), PublishedAt), PublishedAt);

        var feed = RecapFeed.Merge(RecapFeed.Parse(yesterday), RecapFeed.Item(Recap(new DateOnly(2026, 11, 16)), PublishedAt), PublishedAt);

        await Assert.That(Items(feed).Select(Guid)).IsEquivalentTo(["enbref-2026-11-16", "enbref-2026-11-15"]);
    }

    [Test]
    public async Task Merge_replaces_the_item_of_the_same_day()
    {
        var first = RecapFeed.Merge(null, RecapFeed.Item(Recap(new DateOnly(2026, 11, 16), "Avant"), PublishedAt), PublishedAt);

        var feed = RecapFeed.Merge(RecapFeed.Parse(first), RecapFeed.Item(Recap(new DateOnly(2026, 11, 16), "Après"), PublishedAt), PublishedAt);

        var item = Items(feed).Single();
        await Assert.That(item.Element("description")!.Value).Contains("Après");
        await Assert.That(item.Element("description")!.Value).DoesNotContain("Avant");
    }

    [Test]
    public async Task Merge_keeps_the_latest_items_only()
    {
        string? feed = null;
        for (var day = 0; day < RecapFeed.Depth + 5; day++)
        {
            var date = new DateOnly(2026, 10, 1).AddDays(day);
            feed = RecapFeed.Merge(feed is null ? null : RecapFeed.Parse(feed), RecapFeed.Item(Recap(date), PublishedAt), PublishedAt);
        }

        var items = Items(feed!);
        await Assert.That(items).Count().IsEqualTo(RecapFeed.Depth);
        await Assert.That(Guid(items[0])).IsEqualTo($"enbref-{new DateOnly(2026, 10, 1).AddDays(RecapFeed.Depth + 4):yyyy-MM-dd}");
    }

    [Test]
    public async Task Latest_date_is_the_date_of_the_first_item()
    {
        var feed = RecapFeed.Merge(null, RecapFeed.Item(Recap(new DateOnly(2026, 11, 16)), PublishedAt), PublishedAt);

        await Assert.That(RecapFeed.LatestDate(RecapFeed.Parse(feed))).IsEqualTo(new DateOnly(2026, 11, 16));
    }

    [Test]
    public async Task Latest_date_of_an_empty_feed_is_null()
    {
        await Assert.That(RecapFeed.LatestDate(XDocument.Parse("<rss><channel /></rss>"))).IsNull();
    }

    [Test]
    public async Task Parse_rejects_what_is_not_xml()
    {
        await Assert.That(() => RecapFeed.Parse("pas du xml")).Throws<XmlException>();
    }
}
