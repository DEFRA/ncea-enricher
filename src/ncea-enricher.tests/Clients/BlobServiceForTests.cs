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
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "NCEA T&F Vocab v1.1 2024-04-02.xlsx");
        Stream fileStream = File.OpenRead(filePath);
        mockBlobServiceClient = new Mock<BlobServiceClient>();
        mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.Uri).Returns(new Uri(new Uri("https://base-uri-blob-storage"), "relative-uri-blob-storage"));
        mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));
        mockBlobClient.Setup(s => s.OpenReadAsync(It.IsAny<BlobOpenReadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(mockBlobContainerClient.Object);
        mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(mockBlobClient.Object);
        
        var service = new BlobStorageService(mockBlobServiceClient.Object);
        return service;
    }
}
