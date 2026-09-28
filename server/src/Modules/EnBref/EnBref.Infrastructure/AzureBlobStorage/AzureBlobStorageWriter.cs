using static EnBref.Application.Contracts.Constants;

namespace EnBref.Infrastructure.AzureBlobStorage;

public class AzureBlobStorageWriter : IObjectStorageWriter<Recap>
{
    private readonly LocalStorageService _localStorageService;
    private readonly BlobContainerClient _blobContainerClient;
    private const string BlobContainerId = "en-bref-a95eb7fe-d34a-475b-8af4-2f066b2a348c";

    public AzureBlobStorageWriter(Settings settings, LocalStorageService localStorageService)
    {
        var connectionString = settings.EnBrefConnectionString;
        var blobServiceClient = new BlobServiceClient(connectionString);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerId);
        _localStorageService = localStorageService;
    }
    
    public async Task<Uri> StoreObjectAsync(Recap content)
    {
        var archiveFileName = "recap-" + DateTime.Now.ToString("yyyy-MM-dd") + ".json";
        await UploadFile(archiveFileName);
        await UploadFile(LatestRecapFileName);
        
        return new Uri(string.Empty);

        async Task UploadFile(string fileName)
        {
            var jsonContent = JsonSerializer.Serialize(content);
            var localFilePath = await _localStorageService.CreateLocalFileForUploading(jsonContent, fileName);
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(localFilePath, true);
        } 
    }
}