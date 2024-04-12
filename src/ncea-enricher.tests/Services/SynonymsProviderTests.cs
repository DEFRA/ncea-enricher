using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services;
using Ncea.Enricher.Tests.Clients;

namespace Ncea.Enricher.Tests.Services;

public class SynonymsProviderTests
{
    private IServiceProvider _serviceProvider;
    private IBlobStorageService _blobStorageService;

    public SynonymsProviderTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _blobStorageService = BlobServiceForTests.Get();
    }

    [Fact]
    public async Task GetAll()
    {
        //Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>()!;
        var synonymsProvider = new SynonymsProvider(configuration, _blobStorageService);
        
        // Act
        var result = await synonymsProvider.GetAll(It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Classifier>>();
        result.Where(x => x.Level == 1).Count().Should().Be(4);
    }
}
