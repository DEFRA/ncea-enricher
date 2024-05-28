using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using Ncea.Enricher.Infrastructure;

namespace Ncea.Enricher.Tests.Clients;

public static class BlobServiceForTests
{
    public static BlobService Get()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "NCEA T&F Vocab v1.1 2024-04-02.xlsx");
        Stream fileStream = File.OpenRead(filePath);

        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();

        mockBlobClient.Setup(x => x.Uri).Returns(new Uri(new Uri("https://base-uri-blob-storage"), "relative-uri-blob-storage"));
        mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));
        mockBlobClient.Setup(s => s.OpenReadAsync(It.IsAny<BlobOpenReadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        mockBlobClient.Setup(s => s.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));

        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(mockBlobContainerClient.Object);
        mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(mockBlobClient.Object);
        
        var service = new BlobService(mockBlobServiceClient.Object);
        return service;
    }
    public static BlobService Get(out Mock<BlobServiceClient> mockBlobServiceClient,
                                  out Mock<BlobContainerClient> mockBlobContainerClient,
                                  out Mock<BlobClient> mockBlobClient)
    {
        mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.Uri).Returns(new Uri(new Uri("https://base-uri-blob-storage"), "relative-uri-blob-storage"));

        var blobContent = new BinaryData("this is test data");
        var downloadResult = BlobsModelFactory.BlobDownloadResult(content: blobContent);

        // Empty Response mock to merge with since I only care about the downloadResult. Modify at will.
        var response = Response.FromValue(downloadResult, new Mock<Response>().Object);

        mockBlobClient.Setup(x =>
            x.DownloadContentAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
        mockBlobClient.Setup(s => s.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));

        mockBlobServiceClient = new Mock<BlobServiceClient>();
        mockBlobContainerClient = new Mock<BlobContainerClient>();

        var blobContainerInfo = BlobsModelFactory.BlobContainerInfo(It.IsAny<ETag>(), It.IsAny<DateTimeOffset>());
        var mockContainerResponse = Response.FromValue(blobContainerInfo, new Mock<Response>().Object);

        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(mockBlobContainerClient.Object);
        mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(mockBlobClient.Object);
        mockBlobContainerClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, Mock.Of<Response>()));
        mockBlobContainerClient.Setup<Task<Response<BlobContainerInfo>>>(x =>
            x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(),
            It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContainerResponse);
        mockBlobContainerClient.Setup(x =>
            x.DeleteBlobIfExistsAsync(It.IsAny<string>(), It.IsAny<DeleteSnapshotsOption>(),
            It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));

        var blobList = new BlobItem[]
        {
            BlobsModelFactory.BlobItem("Blob1"),
            BlobsModelFactory.BlobItem("Blob2"),
            BlobsModelFactory.BlobItem("Blob3")
        };
        Page<BlobItem> page = Page<BlobItem>.FromValues(blobList, null, Mock.Of<Response>());
        AsyncPageable<BlobItem> pageableBlobList = AsyncPageable<BlobItem>.FromPages(new[] { page });
        mockBlobContainerClient
            .Setup(m => m.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(pageableBlobList);

        var service = new BlobService(mockBlobServiceClient.Object);

        return service;
    }

    public static BlobService GetMdcXml()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        Stream fileStream = File.OpenRead(filePath);

        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();

        mockBlobClient.Setup(x => x.Uri).Returns(new Uri(new Uri("https://base-uri-blob-storage"), "relative-uri-blob-storage"));
        mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));
        mockBlobClient.Setup(s => s.OpenReadAsync(It.IsAny<BlobOpenReadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        mockBlobClient.Setup(s => s.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, new Mock<Response>().Object));

        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(mockBlobContainerClient.Object);
        mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(mockBlobClient.Object);

        var service = new BlobService(mockBlobServiceClient.Object);
        return service;
    }
}
