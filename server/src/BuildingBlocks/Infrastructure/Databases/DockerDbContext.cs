namespace Infrastructure.Databases;

public class DockerDbContext(IConfiguration configuration) : IDbContext
{
    public LiteDatabase Database { get; } = new(configuration.GetConnectionString("DockerLiteDB"));
}