namespace Application.ObjectStorage;

public interface IObjectStorageReader<T>
{
    Task<T> GetObjectContentAsync(string objectName); 
}