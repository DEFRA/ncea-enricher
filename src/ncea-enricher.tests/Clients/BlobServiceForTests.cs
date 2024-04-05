using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using Ncea.Enricher.Infrastructure;

namespace Ncea.Enricher.Tests.Clients;

public static class BlobServiceForTests
{
    private static List<BlobItem> BlobItems = new List<BlobItem>();
    public static BlobStorageService Get(out Mock<BlobServiceClient> mockBlobServiceClient,
                                  out Mock<BlobContainerClient> mockBlobContainerClient,
                                  out Mock<BlobClient> mockBlobClient)
    {
        mockBlobServiceClient = new Mock<BlobServiceClient>();
        mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.Uri).Returns(new Uri(new Uri("https://base-uri-blob-storage"), "relative-uri-blob-storage"));
        
        var blobContainerInfo = BlobsModelFactory.BlobContainerInfo(It.IsAny<ETag>(), It.IsAny<DateTimeOffset>());
        var mockContainerResponse = Response.FromValue(blobContainerInfo, new Mock<Response>().Object);
        
        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(mockBlobContainerClient.Object);
        mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(mockBlobClient.Object);
        var page = Page<BlobItem>.FromValues(BlobItems, continuationToken: null, new Mock<Response>().Object);
        
        mockBlobContainerClient.Setup<Task<Response<BlobContainerInfo>>>(x =>
            x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(),
            It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContainerResponse);
        mockBlobContainerClient.Setup(x =>
            x.DeleteBlobIfExistsAsync(It.IsAny<string>(), It.IsAny<DeleteSnapshotsOption>(),
            It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));
        mockBlobClient.Setup(x => 
            x.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), 
            It.IsAny<CancellationToken>())).Returns(Task.FromResult(AddBlobItem(BlobsModelFactory.BlobItem())));
        mockBlobClient.Setup(x =>
            x.DownloadContentAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Mock<Response<BlobDownloadResult>>().Object));
        var mockBlobItem = AsyncPageable<BlobItem>.FromPages(new[] { page });
        mockBlobContainerClient.Setup(x =>
            x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(mockBlobItem);

        var service = new BlobStorageService(mockBlobServiceClient.Object);

        return service;
    }

    public static Response<BlobContentInfo> AddBlobItem(BlobItem blobItem)
    {
        var blobContentInfo = new Mock<BlobContentInfo>();
        var mockContentResponse = Response.FromValue(blobContentInfo.Object, new Mock<Response>().Object);

        BlobItems.Add(blobItem);
        return mockContentResponse;
    }
}
