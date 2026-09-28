namespace EnBref.Infrastructure.LocalStorage;

public class LocalStorageContext : ILocalStorageContext
{
    public string DownloadPath => @"..\Data\En-Bref\downloads";
    public string UploadPath => @"..\Data\En-Bref\uploads";
}