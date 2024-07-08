using Microsoft.Extensions.Caching.Memory;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class CachedSynonymsProvider : ISynonymsProvider
{
    private const string ClassifierListCacheKey = "ClassifierList";

    private readonly IMemoryCache _memoryCache;
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly int _cacheDurationInMinutes;

    public CachedSynonymsProvider(IMemoryCache memoryCache, ISynonymsProvider synonymsProvider, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _synonymsProvider = synonymsProvider;
        _cacheDurationInMinutes = configuration.GetValue<int>("CacheDurationInMinutes");
    }

    public async Task<List<ClassifierInfo>> GetAll(CancellationToken cancellationToken)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(_cacheDurationInMinutes));

        if (_memoryCache.TryGetValue(ClassifierListCacheKey, out List<ClassifierInfo>? result))
        {
            return result!;
        }

        result = await _synonymsProvider.GetAll(cancellationToken);

        _memoryCache.Set(ClassifierListCacheKey, result, options);

        return result;
    }
}
