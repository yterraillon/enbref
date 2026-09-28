namespace Infrastructure.Databases;

public interface IDbContext
{
    LiteDatabase Database { get; }
}