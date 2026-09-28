namespace EnBref.Application.Contracts;

public interface IRecapSectionMetricRepository
{
    void Add(RecapSectionMetric recapSectionMetric);
    IEnumerable<RecapSectionMetric> GetAll();
    IEnumerable<RecapSectionMetric> GetBetween(DateTime start, DateTime end);
}
