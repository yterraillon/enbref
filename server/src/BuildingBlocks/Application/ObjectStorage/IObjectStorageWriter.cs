namespace Application.ObjectStorage;

public interface IObjectStorageWriter<in T>
{
    Task<Uri> StoreObjectAsync(T content);
}