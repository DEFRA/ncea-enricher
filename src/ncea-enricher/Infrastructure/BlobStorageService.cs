using Azure.Storage.Blobs;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Infrastructure.Contracts;

namespace Ncea.Enricher.Infrastructure;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient) =>
        (_blobServiceClient) = (blobServiceClient);

    public async Task<string> SaveAsync(SaveBlobRequest request, CancellationToken cancellationToken = default)
    {
        var blobContainer = _blobServiceClient.GetBlobContainerClient(request.Container);
        var blobClient = blobContainer.GetBlobClient(request.FileName);
        await blobClient.UploadAsync(request.Blob, true, cancellationToken);        
        return blobClient.Uri.AbsoluteUri;
    }    
}
