namespace EnBref.Infrastructure.LocalStorage;

public interface ILocalStorageContext
{
    string DownloadPath { get; }
    string UploadPath { get; }
}