namespace Application;

public interface IRepository<T> where T : class
{
    T? Get(Guid id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    bool Remove(Guid id);
    void Update(T entity);

}