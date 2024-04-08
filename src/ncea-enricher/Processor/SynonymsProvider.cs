using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processors.Contracts;

namespace Ncea.Enricher.Processor;

public class SynonymsProvider : ISynonymsProvider
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly IBlobStorageService _blobStorageService;

    public SynonymsProvider(IConfiguration configuration, IMemoryCache memoryCache, IBlobStorageService blobStorageService)
    {
        _configuration = configuration;
        _memoryCache = memoryCache;
        _blobStorageService = blobStorageService;
    }

    public async Task<Classifiers> GetAll(CancellationToken cancellationToken)
    {
        var synonymsContainerName = _configuration.GetValue<string>("SynonymsContainerName");
        var synonymsFileName = _configuration.GetValue<string>("SynonymsFileName");
        var classifiers = await _blobStorageService.ReadCsvFileAsync(synonymsContainerName!, synonymsFileName!, cancellationToken);

        return new Classifiers();
    }
}
