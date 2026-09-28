namespace EnBref.Infrastructure.Databases.Dtos;

public class RecapMetricDto
{
    public Guid Id { get; init; }
    public DateTime GeneratedAt { get; init; }
    public List<string> SectionTitles { get; init; } = [];
}
