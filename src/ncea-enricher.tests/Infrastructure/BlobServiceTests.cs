using Azure.Storage.Blobs;
using Moq;
using Ncea.Enricher.Tests.Clients;

namespace Ncea.Enricher.Tests.Infrastructure;

public class BlobServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldCallRequiredBlobServiceMethods()
    {
        // Arrange
        var service = BlobServiceForTests.Get(out Mock<BlobServiceClient> mockBlobServiceClient,
                                              out Mock<BlobContainerClient> mockBlobContainerClient,
                                              out Mock<BlobClient> mockBlobClient);

        // Act
        await service.ReadCsvFileAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None);
        await service.ReadCsvFileAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None);
        await service.ReadCsvFileAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None);

        // Assert
        mockBlobServiceClient.Verify(x => x.GetBlobContainerClient(It.IsAny<string>()), Times.Exactly(3));
        mockBlobContainerClient.Verify(x => x.GetBlobClient(It.IsAny<string>()), Times.Exactly(3));
        mockBlobClient.Verify(x => x.DownloadContentAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
