using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

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

    public async Task<List<ClassifierInfo>> GetAll(CancellationToken cancellationToken)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        if (_memoryCache.TryGetValue(ClassifierListCacheKey, out List<ClassifierInfo>? result))
        {
            return result!;
        }

        result = await _synonymsProvider.GetAll(cancellationToken);

        _memoryCache.Set(ClassifierListCacheKey, result, options);

        return result;
    }
}
