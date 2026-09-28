namespace EnBref.Application.Models;

public class RecapSectionMetric
{
    public Guid Id { get; init; }
    public DateTime GeneratedAt { get; init; }
    public List<string> SectionTitles { get; init; } = [];

    public static RecapSectionMetric Create(IEnumerable<string> sectionTitles) =>
        new()
        {
            Id = Guid.NewGuid(),
            GeneratedAt = DateTime.UtcNow,
            SectionTitles = sectionTitles.ToList()
        };
}
