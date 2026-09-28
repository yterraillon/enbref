using Application;

namespace Infrastructure.Databases;

public class Repository<T, TEntity>(IDbContext context, IMapper mapper) : IRepository<T>
    where T : class
    where TEntity : new()
{
    protected readonly ILiteCollection<TEntity> Collection = context.Database.GetCollection<TEntity>();

    public T? Get(Guid id) => mapper.Map<T>(Collection.FindById(id));

    public IEnumerable<T> GetAll() => mapper.Map<IEnumerable<T>>(Collection.FindAll());

    public void Add(T entity)=> Collection.Insert(mapper.Map<TEntity>(entity));

    public void Update(T entity) => Collection.Update(mapper.Map<TEntity>(entity));
    
    public bool Remove(Guid id) => Collection.Delete(id);
}