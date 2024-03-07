using Azure.Storage.Blobs;
using Moq;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Tests.Clients;

namespace Ncea.Harvester.Tests.Infrastructure;

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
        await service.SaveAsync(new SaveBlobRequest(Stream.Null, "file1.xml", "jncc"), CancellationToken.None);
        await service.SaveAsync(new SaveBlobRequest(Stream.Null, "file2.xml", "jncc"), CancellationToken.None);
        await service.SaveAsync(new SaveBlobRequest(Stream.Null, "file3.xml", "jncc"), CancellationToken.None);

        // Assert
        mockBlobServiceClient.Verify(x => x.GetBlobContainerClient(It.IsAny<string>()), Times.Exactly(3));
        mockBlobContainerClient.Verify(x => x.GetBlobClient(It.IsAny<string>()), Times.Exactly(3));
        mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(),It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
