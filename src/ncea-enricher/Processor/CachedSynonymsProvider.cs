using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processors.Contracts;

namespace Ncea.Enricher.Processor;

public class CachedSynonymsProvider : ISynonymsProvider
{
    private const string ClassifierListCacheKey = "ClassifierList";
    private readonly IMemoryCache _memoryCache;
    private readonly ISynonymsProvider _synonymsProvider;    

    public CachedSynonymsProvider(IMemoryCache memoryCache, ISynonymsProvider synonymsProvider)
    {
        _memoryCache = memoryCache;
        _synonymsProvider = synonymsProvider;
    }

    public async Task<Classifiers> GetAll(CancellationToken cancellationToken)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(10))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

        if (_memoryCache.TryGetValue(ClassifierListCacheKey, out Classifiers result)) return result;

        result = await _synonymsProvider.GetAll(cancellationToken);

        _memoryCache.Set(ClassifierListCacheKey, result, options);

        return result;
    }
}
