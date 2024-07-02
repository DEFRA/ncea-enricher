using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services;
using Ncea.Enricher.Tests.Clients;
using System.Data;

namespace Ncea.Enricher.Tests.Services;

public class SynonymsProviderTests
{
    private IServiceProvider _serviceProvider;
    private IBlobService _blobStorageService;

    public SynonymsProviderTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _blobStorageService = BlobServiceForTests.Get();
    }

    [Fact]
    public async Task GetAll_WhenAllRequiredColumnsExists_ThenReturnListOfClassifiers()
    {
        //Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>()!;
        var synonymsProvider = new SynonymsProvider(configuration, _blobStorageService);
        
        // Act
        var result = await synonymsProvider.GetAll(It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<ClassifierInfo>>();
        result.Where(x => x.Level == 1).Count().Should().Be(4);
    }

    [Fact]
    public async Task GetAll_WhenAllRequiredColumnsNotExists_ThenThrowArgumntException()
    {
        //Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>()!;

        var dataTable = new DataTable();
        dataTable.Columns.Add("L1 ID");
        var dataRow = dataTable.NewRow();
        dataRow.ItemArray = ["test"];
        dataTable.Rows.Add(dataRow);
        var blobStorageServiceMock = new Mock<IBlobService>();
        blobStorageServiceMock.Setup(x => x.ReadExcelFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataTable);
        var synonymsProvider = new SynonymsProvider(configuration, blobStorageServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => synonymsProvider.GetAll(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetAll_WhenAllRequiredColumnsExistsWithIdAsNullValue_ThenReturnEmptyListOfClassifiers()
    {
        //Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>()!;

        var dataTable = new DataTable();
        dataTable.Columns.Add("L1 ID");
        dataTable.Columns.Add("L1 Term");
        var dataRow = dataTable.NewRow();
        dataRow.ItemArray = [null, "test"];
        dataTable.Rows.Add(dataRow);
        var blobStorageServiceMock = new Mock<IBlobService>();
        blobStorageServiceMock.Setup(x => x.ReadExcelFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataTable);
        var synonymsProvider = new SynonymsProvider(configuration, blobStorageServiceMock.Object);

        // Act
        var result = await synonymsProvider.GetAll(It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<ClassifierInfo>>();
        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WhenAllRequiredColumnsExistsWithTermAsNullValue_ThenReturnEmptyListOfClassifiers()
    {
        //Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>()!;

        var dataTable = new DataTable();
        dataTable.Columns.Add("L1 ID");
        dataTable.Columns.Add("L1 Term");
        var dataRow = dataTable.NewRow();
        dataRow.ItemArray = ["test", null];
        dataTable.Rows.Add(dataRow);
        var blobStorageServiceMock = new Mock<IBlobService>();
        blobStorageServiceMock.Setup(x => x.ReadExcelFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataTable);
        var synonymsProvider = new SynonymsProvider(configuration, blobStorageServiceMock.Object);

        // Act
        var result = await synonymsProvider.GetAll(It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<ClassifierInfo>>();
        result.Count.Should().Be(0);
    }
}
