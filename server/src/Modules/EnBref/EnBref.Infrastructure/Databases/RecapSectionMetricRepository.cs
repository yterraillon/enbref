using AutoMapper;
using EnBref.Application.Contracts;
using EnBref.Application.Models;
using EnBref.Infrastructure.Databases.Dtos;
using Infrastructure.Databases;

namespace EnBref.Infrastructure.Databases;

public class RecapSectionMetricRepository(IDbContext context, IMapper mapper)
    : Repository<RecapSectionMetric, RecapMetricDto>(context, mapper), IRecapSectionMetricRepository
{
    private readonly IMapper _mapper = mapper;

    public IEnumerable<RecapSectionMetric> GetBetween(DateTime start, DateTime end)
    {
        var collection = Collection.Query()
            .Where(dto => dto.GeneratedAt >= start && dto.GeneratedAt <= end)
            .ToList();

        return collection.Count != 0
            ? _mapper.Map<IEnumerable<RecapSectionMetric>>(collection)
            : [];
    }
}
