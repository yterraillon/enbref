using EnBref.Infrastructure.Databases.Dtos;
using Infrastructure.Databases;
using LiteDB;

namespace EnBref.Infrastructure.Databases;

public class EnBrefDbContext
{
    public LiteDatabase Database { get; }

    public EnBrefDbContext(IDbContext context)
    {
        Database = context.Database;
        Database.GetCollection<RecapMetricDto>()
            .EnsureIndex(x => x.Id, true);
    }
}
