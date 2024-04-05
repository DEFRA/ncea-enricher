using Azure.Storage.Blobs;
using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Enricher.Infrastructure;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient) =>
        (_blobServiceClient) = (blobServiceClient);

    public async Task<string> ReadCsvFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
    {
        var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainer.GetBlobClient(fileName);
        var content = await blobClient.DownloadContentAsync(cancellationToken);        
        return content.ToString();
    }    
}