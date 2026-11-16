using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using EnBref.Api.Shared;

namespace EnBref.Api.Features.PublishRecapFeed;

/// <summary>
/// Flux du récap (ADR-012) : le flux RSS 2.0 qui reprend les derniers récaps du jour, un item par
/// date. Le flux publié sert lui-même d'historique : chaque publication le relit et y ajoute l'item du jour.
/// </summary>
public static class RecapFeed
{
    /// <summary>Artefact du flux du récap, publié à côté de latest.json.</summary>
    public const string Artifact = "feed.xml";

    /// <summary>URL publique du flux : ne change plus une fois diffusée (pas de redirection sur GitHub Pages).</summary>
    public const string Url = "https://yterraillon.github.io/cdn/en-bref/data/feed.xml";

    /// <summary>Site vitrine (ADR-010) : lien du canal et des items, et lien du back-office.</summary>
    public const string SiteUrl ="https://yterraillon.github.io/pages/enbref/";

    /// <summary>Nombre d'items gardés : environ un mois de récaps du jour.</summary>
    public const int Depth = 30;

    private const string GuidPrefix = "enbref-";

    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly CultureInfo French = new("fr-FR");

    /// <summary>Item du récap : « Récap du 16 novembre 2026 », le récap complet en HTML.</summary>
    public static XElement Item(Recap recap, DateTimeOffset publishedAt) => new("item",
        new XElement("title", $"Récap du {recap.Date.ToString("d MMMM yyyy", French)}"),
        new XElement("link", SiteUrl),
        // Une date, un guid : un récap régénéré le même jour remplace l'item sans doublon chez le lecteur.
        new XElement("guid", new XAttribute("isPermaLink", "false"), Guid(recap.Date)),
        new XElement("pubDate", Rfc822(publishedAt)),
        new XElement("description", new XCData(Html(recap))));

    /// <summary>Le flux complet : l'item en tête, l'item de même date remplacé, tronqué à <see cref="Depth"/>.</summary>
    /// <param name="existing">Flux publié ; null s'il est absent.</param>
    public static string Merge(XDocument? existing, XElement item, DateTimeOffset publishedAt)
    {
        var guid = item.Element("guid")?.Value;
        var previous = existing?.Root?.Element("channel")?.Elements("item")
            .Where(previousItem => previousItem.Element("guid")?.Value != guid) ?? [];

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", Atom),
                new XElement("channel",
                    new XElement("title", "EnBref"),
                    new XElement("link", SiteUrl),
                    new XElement("description", "L'actualité du jour, en bref."),
                    new XElement("language", "fr-FR"),
                    new XElement("lastBuildDate", Rfc822(publishedAt)),
                    new XElement(Atom + "link",
                        new XAttribute("href", Url),
                        new XAttribute("rel", "self"),
                        new XAttribute("type", "application/rss+xml")),
                    previous.Prepend(item).Take(Depth))));

        return Serialize(document);
    }

    /// <exception cref="XmlException">Le flux n'est pas du XML.</exception>
    public static XDocument Parse(string xml) => XDocument.Parse(xml);

    /// <summary>Date du premier item du flux ; null si le flux n'a pas d'item EnBref.</summary>
    public static DateOnly? LatestDate(XDocument feed)
    {
        var guid = feed.Root?.Element("channel")?.Element("item")?.Element("guid")?.Value;
        return guid is not null && guid.StartsWith(GuidPrefix, StringComparison.Ordinal)
               && DateOnly.TryParseExact(guid[GuidPrefix.Length..], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static string Guid(DateOnly date) => $"{GuidPrefix}{date:yyyy-MM-dd}";

    private static string Rfc822(DateTimeOffset moment) => moment.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture);

    // Les catégories dans l'ordre du glossaire, comme le contrat publié (ADR-008). Le texte est encodé :
    // ni balise ni « ]]> » ne peuvent venir d'une brève.
    private static string Html(Recap recap)
    {
        var html = new StringBuilder();
        foreach (var category in Enum.GetValues<Category>())
        {
            if (recap.Briefs.GetValueOrDefault(category) is not { Count: > 0 } briefs)
            {
                continue;
            }

            html.Append($"<h2>{Encode(category.Label())}</h2>");
            foreach (var brief in briefs)
            {
                html.Append($"<p><strong>{Encode(brief.Title)}</strong><br>{Encode(brief.Summary)}</p>");
            }
        }

        return html.ToString();
    }

    // Seuls les caractères spéciaux du HTML : WebUtility.HtmlEncode changerait aussi les accents en entités.
    private static string Encode(string text) => text
        .Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal)
        .Replace("\"", "&quot;", StringComparison.Ordinal);

    private static string Serialize(XDocument document)
    {
        var builder = new StringBuilder();
        using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
        {
            document.Save(writer);
        }

        // StringBuilder impose utf-16 dans la déclaration ; le fichier publié est en UTF-8.
        return builder.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"", StringComparison.Ordinal);
    }
}
