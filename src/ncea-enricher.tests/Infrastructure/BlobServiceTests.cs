using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Moq;
using Ncea.Enricher.Tests.Clients;
using System.Data;

namespace Ncea.Enricher.Tests.Infrastructure;

public class BlobServiceTests
{
    [Fact]
    public async Task ReadExcelFileAsync_ShouldCallRequiredBlobServiceMethods()
    {
        // Arrange
        var service = BlobServiceForTests.Get(out Mock<BlobServiceClient> mockBlobServiceClient,
                                              out Mock<BlobContainerClient> mockBlobContainerClient,
                                              out Mock<BlobClient> mockBlobClient);

        // Act
        var result = await service.ReadExcelFileAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<DataTable>();
        result.Rows.Should().HaveCount(117);
    }
}
