using Azure.Storage.Blobs;
using FluentAssertions;
using Moq;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Tests.Clients;
using System.Data;

namespace Ncea.Enricher.Tests.Infrastructure;

public class BlobServiceTests
{
    [Fact]
    public async Task ReadExcelFileAsync_ShouldCallRequiredBlobServiceMethods()
    {
        // Arrange
        var service = BlobServiceForTests.Get();

        // Act
        var result = await service.ReadExcelFileAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<DataTable>();
        result.Rows.Should().HaveCount(117);
    }

    [Fact]
    public async Task GetContentAsync_WhenBlobsFromHarvesterExists_ReadTheContnetFromBlob()
    {
        // Arrange
        var service = BlobServiceForTests.Get(out Mock<BlobServiceClient> mockBlobServiceClient,
                                              out Mock<BlobContainerClient> mockBlobContainerClient,
                                              out Mock<BlobClient> mockBlobClient);

        // Act
        await service.GetContentAsync(new GetBlobContentRequest(It.IsAny<string>(), It.IsAny<string>()), It.IsAny<CancellationToken>());

        // Assert
        mockBlobServiceClient.Verify(x => x.GetBlobContainerClient(It.IsAny<string>()), Times.Exactly(1));
        mockBlobContainerClient.Verify(x => x.GetBlobClient(It.IsAny<string>()), Times.Exactly(1));
        mockBlobClient.Verify(x => x.DownloadContentAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
