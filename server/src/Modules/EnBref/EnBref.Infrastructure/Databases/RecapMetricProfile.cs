using AutoMapper;
using EnBref.Application.Models;
using EnBref.Infrastructure.Databases.Dtos;

namespace EnBref.Infrastructure.Databases;

public class RecapMetricProfile : Profile
{
    public RecapMetricProfile()
    {
        CreateMap<RecapMetricDto, RecapSectionMetric>().ReverseMap();
    }
}
