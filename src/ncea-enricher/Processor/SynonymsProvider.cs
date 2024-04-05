using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processors.Contracts;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Ncea.Enricher.Processor;

public class SynonymsProvider : ISynonymsProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly IBlobStorageService _blobStorageService;

    public SynonymsProvider(IMemoryCache memoryCache, IBlobStorageService blobStorageService)
    {
        _memoryCache = memoryCache;
        _blobStorageService = blobStorageService;
    }

    public async Task<IList<Classifier>> GetAll(CancellationToken cancellationToken)
    {
        var content = await _blobStorageService.ReadCsvFileAsync("", "", cancellationToken);
        return JsonConvert.DeserializeObject<IList<Classifier>>(content)!;
    }
}
