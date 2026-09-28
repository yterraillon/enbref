namespace EnBref.Infrastructure.LocalStorage;

public class DockerLocalStorageContext : ILocalStorageContext
{
    public string DownloadPath => "/data/en-bref/downloads";
    public string UploadPath => "/data/en-bref/uploads";
}