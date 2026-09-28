namespace EnBref.Infrastructure.AzureBlobStorage;

public class AzureBlobStorageReader : IObjectStorageReader<Recap>
{
    private readonly LocalStorageService _localStorageService;
    private readonly BlobContainerClient _blobContainerClient;
    private const string BlobContainerId = "en-bref-a95eb7fe-d34a-475b-8af4-2f066b2a348c";

    public AzureBlobStorageReader(Settings settings, LocalStorageService localStorageService)
    {
        var connectionString = settings.EnBrefConnectionString;
        var blobServiceClient = new BlobServiceClient(connectionString);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerId);
        _localStorageService = localStorageService;
    }
    
    public async Task<Recap> GetObjectContentAsync(string objectName)
    {
        var downloadFilePath = _localStorageService.GetLocalDownloadPath(objectName);
        var blobClient = _blobContainerClient.GetBlobClient(objectName);
        await blobClient.DownloadToAsync(downloadFilePath);

        var content = await LocalStorageService.ReadFileAsync(downloadFilePath);
        return JsonSerializer.Deserialize<Recap>(content) ?? Recap.EmptyRecap;
    }
}