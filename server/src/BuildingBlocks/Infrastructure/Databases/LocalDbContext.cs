namespace Infrastructure.Databases;

public class LocalDbContext(IConfiguration configuration) : IDbContext
{
    public LiteDatabase Database { get; } = new(configuration.GetConnectionString("LocalLiteDB"));
}