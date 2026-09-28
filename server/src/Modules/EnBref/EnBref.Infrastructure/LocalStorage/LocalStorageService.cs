namespace EnBref.Infrastructure.LocalStorage;

public class LocalStorageService(ILocalStorageContext localStorageContext)
{
    public string GetLocalDownloadPath(string objectName)
    {
        var localPath = localStorageContext.DownloadPath;
        Directory.CreateDirectory(localPath);
        return Path.Combine(localPath, objectName);
    }
    
    public async Task<string> CreateLocalFileForUploading(string text, string fileName)
    {
        var localPath = localStorageContext.UploadPath;
        Directory.CreateDirectory(localPath);
        
        var localFilePath = Path.Combine(localPath, fileName);
        await File.WriteAllTextAsync(localFilePath, text);

        return localFilePath;
    }
    
    public static async Task<string> ReadFileAsync(string filePath)
    {
        if (!File.Exists(filePath)) 
            throw new FileNotFoundException("The specified file was not found.", filePath);
        using var reader = new StreamReader(filePath);
        return await reader.ReadToEndAsync();
    }
}
