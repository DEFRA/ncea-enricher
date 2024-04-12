using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;

namespace Ncea.Enricher.Tests.Services;

public class CachedSynonymsProviderTests
{
    private IServiceProvider _serviceProvider;
    private IBlobStorageService _blobStorageService;
    private ISynonymsProvider _synonymsProvider;

    public CachedSynonymsProviderTests()
    {        
        _serviceProvider = ServiceProviderForTests.Get();
        var configuration = _serviceProvider.GetService<IConfiguration>()!;
        _blobStorageService = BlobServiceForTests.Get();
        _synonymsProvider = new SynonymsProvider(configuration, _blobStorageService);
    }

    [Fact]
    public async Task GetAll()
    {
        //Arrange
        var cache = _serviceProvider.GetService<IMemoryCache>()!;
        var cachedSynonymsProvider = new CachedSynonymsProvider(cache, _synonymsProvider);

        // Act
        var result = await cachedSynonymsProvider.GetAll(It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Classifier>>();
        result.Where(x => x.Level == 1).Count().Should().Be(4);
    }

}
